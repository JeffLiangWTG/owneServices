using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;
using OrgSupplierPart = Enterprise.Customs.CA.Business.OrgSupplierPart;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class TCTPRUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMemBerForMakeTextBox()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();

			pivot.CI_ChildType = CA.Business.ClassificationTypeList.Codes.HTI;
			pivot.CI_FormattedTariffNum = "3333333333";
			pivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			pivot.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;

			using (var tprControl = new TCTPRUserControl(false))
			{
				tprControl.SetDataBinding(pivot.TCPGAHeader, "");
				var member = tprControl.BindingSource.GetBindingMember(tprControl.FindSingle<ZTextBox>("MakeTextBox"));
				AssertEquals("JI_BrandName", member);
			}

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			using (var tprControl = new TCTPRUserControl(true))
			{
				tprControl.SetDataBinding(invoiceLine.TCPGAHeader, "");
				var member = tprControl.BindingSource.GetBindingMember(tprControl.FindSingle<ZTextBox>("MakeTextBox"));
				AssertEquals("JI_BrandName", member);
			}
		}

		public void TestUpdateDeclarationCheckBoxesCaptions_WithNoException()
		{
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC, "40111000", "TPR");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.JobComInvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				control.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceline);
				invoiceline.CA_TCInd = "Y";
				var requirments = invoiceline.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.TC);
				var programTPR = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == TCPGADepartmentCodes.Codes.TPR);
				programTPR.Indicator = "Y";

				ZTabPage tabPageTC = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(x => x.Text == PGACodes.Codes.TC && x.TabVisible);
				control.LineDetailTabControl.SelectedTab = tabPageTC;
				var userControl = tabPageTC.FindSingleOrDefault<TCUserControl>();
				var tabpageTPR = userControl.FindSingleOrDefault<ZTabPage>(x => x.Text == TCPGADepartmentCodes.Descriptions.TPR && x.TabVisible);
				var userControlTPR = tabpageTPR.FindSingleOrDefault<TCTPRUserControl>();

				var importerDeclarationCheckBox = userControlTPR.Controls.Find("ImporterDeclarationCheckBox", true)[0] as ZCheckBox;
				var importerDeclarationStateDescLabel = userControlTPR.Controls.Find("ImporterDeclarationStateDescLabel", true)[0] as ZLabel;
				Assert(!importerDeclarationCheckBox.Visible);
				Assert(!importerDeclarationStateDescLabel.Visible);
				Assert(!invoiceline.TCPGAHeader.ZZImporterDeclarationVisibility);

				invoiceline.JI_FormattedTariff = "4011.10.00";
				invoiceline.TCPGAHeader.CA_ProductClass = "TC01";
				invoiceline.TCPGAHeader.CA_ProductType = "TC04";
				invoiceline.TCPGAHeader.CA_ImportReasonCode = "TC01";

				Assert(invoiceline.TCPGAHeader.ZZImporterDeclarationVisibility);
				Assert(importerDeclarationCheckBox.Visible);
				Assert(importerDeclarationStateDescLabel.Visible);
				AssertEquals("TC01 - All Countries", importerDeclarationCheckBox.GetExtension<ILabelCaptionRenderer>().Caption);

				invoiceline.CA_TCInd = "Z";
				Assert(invoiceline.TCPGAHeader.IsNull);

				AssertNoExceptionThrown(delegate
				{
					userControlTPR.UpdateDeclarationCheckBoxesCaptions();
				});
			}
		}
	}
}
