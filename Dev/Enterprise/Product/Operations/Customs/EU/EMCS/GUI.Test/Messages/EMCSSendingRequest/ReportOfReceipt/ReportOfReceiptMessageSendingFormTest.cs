using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(ReportOfReceiptMessageSendingForm))]
	sealed class ReportOfReceiptMessageSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => GetEMCSSendReportOfReceiptForm();

		protected override bool AllowHasChangesOnFormOpen => true;

		public void TestFormHeading()
		{
			using (var form = (ZForm)GetFormToBashCore())
			{
				AssertEquals("Sending Messages", form.FormHeading);
			}
		}

		public void TestTypeOfBusinessEntity()
		{
			using (var form = GetEMCSSendReportOfReceiptForm())
			{
				AssertType<ReportOfReceiptSendingActionParent>(form.BusinessEntity);
			}
		}

		public void TestBottomSectionUserControlType()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var messageSendingObjectParent = new ReportOfReceiptSendingActionParent(declaration);
			using (var form = new ReportOfReceiptMessageSendingForm_ForTest(messageSendingObjectParent))
			{
				AssertEquals("pre-condition", 1, form.WarningSplitContainer.Panel2.Controls.Count);
				AssertType<ReportOfReceiptBottomSectionUserControl>(form.WarningSplitContainer.Panel2.Controls[0]);
			}
		}

		ReportOfReceiptMessageSendingForm GetEMCSSendReportOfReceiptForm()
		{
			var declaration = Factory.NewWithValidTestData<EMCSJobDeclaration>();
			declaration.JE_MessageType = EMCSJobDeclaration.EMCSMessageTypeCode;
			var messageSendingObjectParent = new ReportOfReceiptSendingActionParent(declaration);
			return new ReportOfReceiptMessageSendingForm(messageSendingObjectParent);
		}

		class ReportOfReceiptMessageSendingForm_ForTest : ReportOfReceiptMessageSendingForm
		{
			public ReportOfReceiptMessageSendingForm_ForTest(ReportOfReceiptSendingActionParent parent) : base(parent)
			{
			}

			new public KSplitContainer WarningSplitContainer => base.WarningSplitContainer;
		}
	}
}
