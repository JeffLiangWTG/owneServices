using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestVisible_ProduceType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPDetailsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var customsConsigneeNameTextBox = ctr.FindSingle<ZTextBox>("QH_CustomsConsigneeNameTextBox");
				var exemptionCodeTextBox = ctr.FindSingle<ZTextBox>("QH_ExemptionCodeTextBox");
				var isRegionCodeFindBox = ctr.FindSingle<ZCodeFindBox>("QH_AQISRegionCodeFindBox");
				Assert("Precondition", !invoiceHeader.IsNEXDOCSActive);
				Assert("QH_CustomsConsigneeNameTextBox should be invisible when the invoice is EXDOCS", !customsConsigneeNameTextBox.Visible);
				Assert("QH_ExemptionCodeTextBox should be invisible when the invoice is EXDOCS", !exemptionCodeTextBox.Visible);
				Assert("QH_AQISRegionCodeFindBox should be visible when the invoice is EXDOCS", isRegionCodeFindBox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert("Precondition", invoiceHeader.IsNEXDOCSActive);
				Assert("QH_CustomsConsigneeNameTextBox should be visible when the invoice is NEXDOCS", customsConsigneeNameTextBox.Visible);
				Assert("QH_ExemptionCodeTextBox should be visible when the invoice is NEXDOCS", exemptionCodeTextBox.Visible);
				Assert("QH_AQISRegionCodeFindBox should be invisible when the invoice is NEXDOCS", !isRegionCodeFindBox.Visible);
			}
		}

		public void TestVisible_PrintLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPDetailsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var textBox = ctr.FindSingle<ZTextBox>("QH_CertificateRequiredLocationTextBox");
				var codeFindBox = ctr.FindSingle<ZCodeFindBox>("QH_CertificateRequiredLocationCodeFindBox");
				var guidFindBox = ctr.FindSingle<ZGuidFindBox>("QH_OH_PrintLocationOrganisationGuidFindBox");
				invoiceHeader.QuarantineExDocHeader.QH_PrintLocation = EXDOCCodeOrganisation.Codes.Code;
				Assert("QH_CertificateRequiredLocationTextBox should be visible when the QH_PrintLocation is CODE.", textBox.Visible);
				Assert("QH_CertificateRequiredLocationCodeFindBox should be invisible when the QH_PrintLocation is CODE.", !codeFindBox.Visible);
				Assert("QH_OH_PrintLocationOrganisationGuidFindBox should be invisible when the QH_PrintLocation is CODE.", !guidFindBox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_PrintLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
				Assert("QH_CertificateRequiredLocationTextBox should be invisible when the QH_PrintLocation is AqisPlaceCode.", !textBox.Visible);
				Assert("QH_CertificateRequiredLocationCodeFindBox should be visible when the QH_PrintLocation is AqisPlaceCode.", codeFindBox.Visible);
				Assert("QH_OH_PrintLocationOrganisationGuidFindBox should be invisible when the QH_PrintLocation is AqisPlaceCode.", !guidFindBox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_PrintLocation = EXDOCCodeOrganisation.Codes.Organisation;
				Assert("QH_CertificateRequiredLocationTextBox should be invisible when the QH_PrintLocation is Organisation.", !textBox.Visible);
				Assert("QH_CertificateRequiredLocationCodeFindBox should be invisible when the QH_PrintLocation is Organisation.", !codeFindBox.Visible);
				Assert("QH_OH_PrintLocationOrganisationGuidFindBox should be visible when the QH_PrintLocation is Organisation.", guidFindBox.Visible);
			}
		}
	}
}
