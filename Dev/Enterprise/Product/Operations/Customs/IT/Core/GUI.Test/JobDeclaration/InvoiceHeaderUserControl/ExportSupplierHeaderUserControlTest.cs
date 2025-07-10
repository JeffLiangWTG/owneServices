using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ExportSupplierHeaderUserControl))]
sealed class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new ExportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			control.Controls.Find("SupportingDocumentsTabPage", true).First().Show();
			var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(InvoiceLayoutSupportingDocumentsUserControl), supportingDocument.UserControlType);

			control.Controls.Find("PreviousDocumentsTabPage", true).First().Show();
			var previousDocument = control.Controls.Find("previousDocumentsUserControl1", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(PreviousDocumentsUserControl), previousDocument.UserControlType);
		}
	}

	public void TestDisableImportDataMenuItem()
	{
		using (var form = new ZForm())
		using (var control = new ExportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var invoiceHeaderGrid = control.FindSingle<ZGrid>("Grid");
			AssertEquals("Invoice Header Grid, DisableImportDataMenuItem", false, invoiceHeaderGrid.DisableImportDataMenuItem);
		}
	}

	public void TestOrganisationTabVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		declaration.Invoices
			.AddNew()
			.InvoiceLines
			.AddNew()
			.JI_CEI = entryInstruction1.PK;

		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();

			var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
			var mainTabControl = customsBrokerageUserControl.MainTabControl;
			var invoiceLineTabPage = customsBrokerageUserControl.InvoicesTabPage;

			mainTabControl.SelectedTab = invoiceLineTabPage;
			var invoiceHeaderUserControl = invoiceLineTabPage.FindSingle<ExportSupplierHeaderUserControl>();

			AssertOrganisationTabIsVisible(invoiceHeaderUserControl, "When ParticipantType is empty, InvoiceTabControl should not contain Organisation Tab", false);

			entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			ChangeTab();
			AssertOrganisationTabIsVisible(invoiceHeaderUserControl, "When ParticipantType is BuyersConsol, InvoiceTabControl should contain Organisation Tab", true);

			entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
			ChangeTab();
			AssertOrganisationTabIsVisible(invoiceHeaderUserControl, "When ParticipantType is not BuyersConsol, InvoiceTabControl should not contain Organisation Tab", false);

			void ChangeTab()
			{
				mainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				mainTabControl.SelectedTab = invoiceLineTabPage;
			}
		}
	}

	public void TestOrganizationTabVisibility_ForwardingShipment()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new ZForm(declaration))
		using (var control = new ExportSupplierHeaderUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();

			AssertOrganisationTabIsVisible(control, "When Shipment is not associated with the declaration", true);

			declaration.JE_JS = shipment.PK;

			form.Close();
			form.Show();

			AssertOrganisationTabIsVisible(control, "When Shipment is associated with the declaration", false);
		}
	}

	public void TestAdditionalInfoUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new ExportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			control.Controls.Find("AdditionalInfoTabPage", true).First().Show();
			var additionalInfoUserControl = control.Controls.Find("additionalInfosUserControl1", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(InvoiceHeaderAdditionalInfoUserControlWithGrid), additionalInfoUserControl.UserControlType);
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit", "InvoiceCurrLandedCostExRateCalcEdit" }).Union(new[] { "AgreedPlaceCodeFindBox", "TransportChargesMethodOfPaymentDropEdit" });

	void AssertOrganisationTabIsVisible(ExportSupplierHeaderUserControl control, string assertionMessage, bool expectedVisible)
	{
		var organisationTabPage = (ZTabPage)control.InvoiceTabControl.TabPages["OrganisationsTabPage"];
		if (!expectedVisible)
		{
			AssertNull("OrganisationTab", organisationTabPage);
		}
		else
		{
			AssertNotNull("OrganisationTab", organisationTabPage);
			AssertEquals(assertionMessage, expectedVisible, organisationTabPage.TabVisible);
		}
	}
}
