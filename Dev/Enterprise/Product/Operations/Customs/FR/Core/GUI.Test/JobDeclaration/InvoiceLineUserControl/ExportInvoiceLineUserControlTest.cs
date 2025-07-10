using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(ExportInvoiceLineUserControl))]
	class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestZG_TransNatureVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;

			using (var form = new JobDeclarationForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				var transactionNatureDropEdit = control.FindSingle<ZDropEdit>("TransactionNatureDropEdit");
				AssertEquals("[24] Tran. Nature should be invisible when JE_ApplicationCode is Delta G.", false, transactionNatureDropEdit.Visible);
			}

			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new JobDeclarationForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				var transactionNatureDropEdit = control.FindSingle<ZDropEdit>("TransactionNatureDropEdit");
				Assert("[24] Tran. Nature should be visible when JE_ApplicationCode is Delta IE.", transactionNatureDropEdit.Visible);
			}
		}

		public void TestGetPreviousDocumentsUserControlType()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				var previousDocumentsUserControlType = typeof(ExportInvoiceLineUserControl).GetMethod("GetPreviousDocumentsUserControlType", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertEquals(typeof(PreviousDocumentsUserControl), previousDocumentsUserControlType.Invoke(control, Array.Empty<object>()));
			}
		}

		[RequiresSTA]
		public void TestUniversalTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("IMP", control.GetUniversalTariffType());

				control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				AssertEquals("IMP", control.GetUniversalTariffType());

				declaration.JE_TariffType = "";
				control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 0;
				AssertEquals("EXP", control.GetUniversalTariffType());

				control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				AssertEquals("EXP", control.GetUniversalTariffType());
			}
		}

		public void TestCountryOfDestinationFindBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				var countryOfDestinationFindBox = control.Controls.Find("CountryOfDestinationCodeFindBox", true).FirstOrDefault();
				AssertNull("CountryOfDestinationCodeFindBox should be invisible", countryOfDestinationFindBox);
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.Controls.Find("SupportingDocumentsTabPage", true).First().Show();
				var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(SupportingDocumentsUserControl), supportingDocument.UserControlType);

				var tabPage = (ZTabPage)control.Controls.Find("NationalAdditionalCodeTabPage", true).FirstOrDefault();
				AssertNull(tabPage);

				var tariffCodeCtrl = control.Controls.Find("tariffBypassCodeControl", true).FirstOrDefault();
				AssertNull("TariffBypassCode control should not be found", tariffCodeCtrl);

				AssertNotNull(control.PreviousEntryNumberTextBox);
				AssertNotNull(control.PreviousEntryLineNumberCalcEdit);
				AssertNotNull(control.BondedWhsQuantityCalcDropEdit);
			}
		}

		public void TestPaymentMethodShouldBeRemoved()
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var taxGrid = (ZGrid)control.Controls.Find("TaxGrid", true).Single();
				AssertNull("", taxGrid.GetColumnStyle("Data+G4_MethodOfPayment"));

				var methodOfPaymentDropEdit = control.Controls.Find("MethodOfPaymentDropEdit", true).Single();
				Assert(!methodOfPaymentDropEdit.Visible);
			}
		}

		public void TestGDMLinkTabIndex()
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var gdmLink = control.Controls.Find("GDMLink", true).Single();
				AssertEquals(9, gdmLink.TabIndex);
			}
		}

		public void TestFRGuidedDecisionMakingFormShownWhenGuidedDecisionMakingButtonClicked()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var gDMLink = control.FindSingle<ZLinkLabel>("GDMLink");
				gDMLink.PerformClick_ForTest();

				var gDMForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("GDM Form should be created and shown", () =>
				{
					AssertNotNull("GDM Form created", gDMForm);
					AssertType<GDM.GuidedDecisionMakingForm>(gDMForm);
				});
			}
		}

		public void TestPreviousEntryNumberIsNotVisibleWhenIntoWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("Y", "N", control =>
			{
				AssertEquals("Previous entry No. is not visible when procedure is into warehouse.", false, control.PreviousEntryNumberTextBox.Visible);
				AssertEquals("Previous entry No. is not visible when procedure is into warehouse.", false, control.PreviousEntryLineNumberCalcEdit.Visible);
			});
		}

		public void TestPreviousEntryNumberIsVisibleWhenOutOfWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("N", "Y", control =>
			{
				AssertEquals("Previous entry No. is visible when procedure is out of warehouse.", true, control.PreviousEntryNumberTextBox.Visible);
				AssertEquals("Previous entry No. is visible when procedure is out of warehouse.", true, control.PreviousEntryLineNumberCalcEdit.Visible);
			});
		}

		public void TestBondedWhsQuantityIsVisibleWhenIntoWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("Y", "N", control =>
			{
				AssertEquals("Bonded warehouse quantity is visible when procedure is into warehouse.", true, control.BondedWhsQuantityCalcDropEdit.Visible);
			});
		}

		public void TestBondedWhsQuantityIsVisibleWhenOutOfWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("N", "Y", control =>
			{
				AssertEquals("Bonded warehouse quantity is visible when procedure is out of warehouse.", true, control.BondedWhsQuantityCalcDropEdit.Visible);
			});
		}

		public void AssertVisibilityWhenProcedureChanges(string intoWhs, string outOfWhs, Action<ExportInvoiceLineUserControl> assertion)
		{
			RefDataHelper.AddProcedure(Factory, EU.Business.MessageTypeList.Codes.Export, "12", "34", "567", intoWhs, outOfWhs);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "1234567";

				using (var form = new ZForm(declaration))
				using (var control = new ExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					control.JobDeclaration = declaration;
					form.Show();

					assertion(control);
				}
			}
		}

		public void TestAdditionalInfosTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Special Mentions", typeof(AdditionalInfosUserControl));
		}
	}

	public class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
	{
		public new Func<ZString> GetUniversalTariffType => base.GetUniversalTariffType;
	}
}
