using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPMessagingUserControlTest : TestCaseWithFactory
	{
		public void TestRequestForPermitNumberTextBoxCaption()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(quarantineDeclaration))
			using (var ctrl = new RFPMessagingUserControl())
			{
				form.Controls.Add(ctrl);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctrl, "Invoices");
				form.Show();
				Application.DoEvents();
				quarantineDeclaration.IsAQISCertificateRequest = true;
				var requestForPermitNumberTextBox = ctrl.FindSingle<ZTextBox>("QH_RequestForPermitNumberTextBox");
				AssertEquals("Certificate Identification", requestForPermitNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				quarantineDeclaration.IsAQISCertificateRequest = false;
				AssertEquals(false, invoiceHeader.IsNEXDOCSActive);
				AssertEquals("RFP Number", requestForPermitNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				AssertEquals(true, invoiceHeader.IsNEXDOCSActive);
				AssertEquals("REX Number", requestForPermitNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestMessageGridColumns()
		{
			quarantineDeclaration.IsAQISCertificateRequest = true;
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			using (var form = new ZForm(quarantineDeclaration))
			using (var ctrl = new RFPMessagingUserControl())
			{
				form.Controls.Add(ctrl);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctrl, "Invoices");
				form.Show();
				Application.DoEvents();
				AssertEquals("IsNEXDOCSActive", true, invoiceHeader.IsNEXDOCSActive);
				var messageCollectionGrid = ctrl.FindSingle<ZGrid>("MessageCollectionGrid");
				AssertEquals(true, messageCollectionGrid.Enabled);
				AssertEquals(true, messageCollectionGrid.Visible);
				CombineAssertions(() =>
				{
					AssertEquals("EM_SendOrReceiveHumanReadable", true, messageCollectionGrid.GetColumnStyle("EM_SendOrReceiveHumanReadable") != null);
					AssertEquals("EM_MessageNum", true, messageCollectionGrid.GetColumnStyle("EM_MessageNum") != null);
					AssertEquals("EM_InterchangeNumber", true, messageCollectionGrid.GetColumnStyle("EM_InterchangeNumber") != null);
					AssertEquals("EM_DateTimeInterchangeSent", true, messageCollectionGrid.GetColumnStyle("EM_DateTimeInterchangeSent") != null);
					AssertEquals("EM_MessageType", true, messageCollectionGrid.GetColumnStyle("EM_MessageType") != null);
					AssertEquals("EM_Status", true, messageCollectionGrid.GetColumnStyle("EM_Status") != null);
					AssertEquals("EM_User", true, messageCollectionGrid.GetColumnStyle("EM_User") != null);
					AssertEquals("EM_InterchangeStatus", true, messageCollectionGrid.GetColumnStyle("EM_InterchangeStatus") != null);
					AssertEquals("eHubID", true, messageCollectionGrid.GetColumnStyle("Interchange+eHubID") != null);
					var subTypeColumn = messageCollectionGrid.GetColumnStyle("EM_MessageSubType");
					AssertEquals("EM_MessageSubType", true, subTypeColumn != null);
					AssertEquals("MessageSubType_GroupName", "Message Sub Type", subTypeColumn.GroupName.Caption);
					Assert("SubType should not be visible by default", !subTypeColumn.IsVisible);
					var subTypeDescriptionColumn = messageCollectionGrid.GetColumnStyle("EM_MessageSubTypeDescription");
					AssertEquals("EM_MessageSubTypeDescription", true, subTypeDescriptionColumn != null);
					AssertEquals("MessageSubTypeDescription_GroupName", "Message Sub Type", subTypeDescriptionColumn.GroupName.Caption);
					Assert("SubTypeDescription should not be visible by default", !subTypeDescriptionColumn.IsVisible);
				});
			}
		}

		JobComInvoiceHeader invoiceHeader;
		JobDeclaration quarantineDeclaration;
		protected override void SetUp()
		{
			base.SetUp();
			quarantineDeclaration = Factory.New<JobDeclaration>();
			quarantineDeclaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			invoiceHeader = quarantineDeclaration.Invoices.AddNew();
		}
	}
}
