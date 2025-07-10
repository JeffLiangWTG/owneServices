using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPForwardTransferUserControlTest : TestCaseWithFactory
	{
		public void TestVisible_ProduceType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPForwardTransferUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var requiresAcceptanceCheckbox = ctr.FindSingle<ZCheckBox>("RequiresAcceptanceCheckbox");
				Assert("Precondition", !invoiceHeader.IsNEXDOCSActive);
				Assert("RequiresAcceptanceCheckbox should be invisible when the invoice is EXDOCS", !requiresAcceptanceCheckbox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert("Precondition", invoiceHeader.IsNEXDOCSActive);
				Assert("requiresAcceptanceCheckbox should be visible when the invoice is NEXDOCS", requiresAcceptanceCheckbox.Visible);
			}
		}

		public void TestVisible_ForwardLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPForwardTransferUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var textBox = ctr.FindSingle<ZTextBox>("QH_ForwardeeEDIUserIdentifierTextBox");
				var guidFindBox = ctr.FindSingle<ZGuidFindBox>("QH_OH_ForwardLocationOrganisationGuidFindBox");
				invoiceHeader.QuarantineExDocHeader.QH_ForwardLocation = EXDOCCodeOrganisation.Codes.Code;
				Assert("QH_ForwardeeEDIUserIdentifierTextBox should be visible when the QH_ForwardLocation is CODE.", textBox.Visible);
				Assert("QH_OH_ForwardLocationOrganisationGuidFindBox should be invisible when the QH_ForwardLocation is CODE.", !guidFindBox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_ForwardLocation = EXDOCCodeOrganisation.Codes.Organisation;
				Assert("QH_ForwardeeEDIUserIdentifierTextBox should be invisible when the QH_ForwardLocation is Organisation.", !textBox.Visible);
				Assert("QH_OH_ForwardLocationOrganisationGuidFindBox should be visible when the QH_ForwardLocation is Organisation.", guidFindBox.Visible);
			}
		}

		public void TestVisible_TransferEDIUserLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPForwardTransferUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var textBox = ctr.FindSingle<ZTextBox>("QH_TransfereeEDIUserIdentifierTextBox");
				var guidFindBox = ctr.FindSingle<ZGuidFindBox>("QH_OH_TransferEDIUserLocationOrganisationGuidFindBox");
				invoiceHeader.QuarantineExDocHeader.QH_TransferEDIUserLocation = EXDOCCodeOrganisation.Codes.Code;
				Assert("QH_TransfereeEDIUserIdentifierTextBox should be visible when the QH_TransferEDIUserLocation is CODE.", textBox.Visible);
				Assert("QH_OH_TransferEDIUserLocationOrganisationGuidFindBox should be invisible when the QH_TransferEDIUserLocation is CODE.", !guidFindBox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_TransferEDIUserLocation = EXDOCCodeOrganisation.Codes.Organisation;
				Assert("QH_TransfereeEDIUserIdentifierTextBox should be invisible when the QH_TransferEDIUserLocation is Organisation.", !textBox.Visible);
				Assert("QH_OH_TransferEDIUserLocationOrganisationGuidFindBox should be visible when the QH_TransferEDIUserLocation is Organisation.", guidFindBox.Visible);
			}
		}

		public void TestVisible_TransferExporterLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPForwardTransferUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var textBox = ctr.FindSingle<ZTextBox>("QH_TransfereeExporterNumberTextBox");
				var guidFindBox = ctr.FindSingle<ZGuidFindBox>("QH_OH_TransferExporterLocationOrganisationGuidFindBox");
				invoiceHeader.QuarantineExDocHeader.QH_TransferExporterLocation = EXDOCCodeOrganisation.Codes.Code;
				Assert("QH_TransfereeExporterNumberTextBox should be visible when the QH_TransferExporterLocation is CODE.", textBox.Visible);
				Assert("QH_OH_TransferExporterLocationOrganisationGuidFindBox should be invisible when the QH_TransferExporterLocation is CODE.", !guidFindBox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_TransferExporterLocation = EXDOCCodeOrganisation.Codes.Organisation;
				Assert("QH_TransfereeExporterNumberTextBox should be invisible when the QH_TransferExporterLocation is Organisation.", !textBox.Visible);
				Assert("QH_OH_TransferExporterLocationOrganisationGuidFindBox should be visible when the QH_TransferExporterLocation is Organisation.", guidFindBox.Visible);
			}
		}
	}
}
