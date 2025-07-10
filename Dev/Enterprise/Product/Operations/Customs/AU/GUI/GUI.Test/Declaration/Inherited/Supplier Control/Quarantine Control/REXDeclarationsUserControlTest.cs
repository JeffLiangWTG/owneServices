using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class REXDeclarationsUserControlTest : TestCaseWithFactory
	{
		public void TestIndicatorText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			using (var testForm = new ZForm(declaration))
			using (var ctr = new REXDeclarationsUserControl())
			{
				testForm.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(ctr, "Invoices");
				testForm.Show();
				Application.DoEvents();
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
				var importedProductFlagCaption = ctr.FindSingle<ZLabel>("REX_QH_ImportedProductFlagCaption");
				AssertEquals("Are any of the products listed in the RFP imported?", importedProductFlagCaption.Text);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
				AssertEquals("Do any of the products listed in this RFP contain imported dairy ingredients, other than from New Zealand?", importedProductFlagCaption.Text);
			}
		}

		public void TestVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new REXDeclarationsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var legallyImportedFlagCaption = ctr.FindSingle<ZLabel>("QH_LegallyImportedFlagCaption");
				var legallyImportedFlagDropEdit = ctr.FindSingle<ZDropEdit>("QH_LegallyImportedFlagDropEdit");
				Assert("Precondition", !invoiceHeader.IsNEXDOCSActive);
				Assert("QH_LegallyImportedFlagCaption should be invisible when the invoice is EXDOCS", !legallyImportedFlagCaption.Visible);
				Assert("QH_LegallyImportedFlagDropEdit should be invisible when the invoice is EXDOCS", !legallyImportedFlagDropEdit.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert("Precondition", invoiceHeader.IsNEXDOCSActive);
				Assert("QH_LegallyImportedFlagCaption should be visible when the invoice is NEXDOCS", legallyImportedFlagCaption.Visible);
				Assert("QH_LegallyImportedFlagDropEdit should be visible when the invoice is NEXDOCS", legallyImportedFlagDropEdit.Visible);
			}
		}
	}
}
