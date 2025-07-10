using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(ReasonForShortageMessageSendingForm))]
	sealed class ReasonForShortageMessageSendingFormTest : ZFormBasherTest
	{
		public void TestMinimumSize()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(800, 555, true), form.MinimumSize);
			}
		}

		public void TestBottomSectionUserControlType()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var messageSendingObjectParent = new ReasonForShortageSendingActionParent(declaration);
			using (var form = new ReasonForShortageMessageSendingForm_ForTest(messageSendingObjectParent))
			{
				AssertEquals("pre-condition", 1, form.WarningSplitContainer.Panel2.Controls.Count);
				AssertType<ReasonForShortageBottomSectionUserControl>(form.WarningSplitContainer.Panel2.Controls[0]);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<EMCSJobDeclaration>();
			declaration.JE_MessageType = EMCSJobDeclaration.EMCSMessageTypeCode;
			var messageSendingObjectParent = new ReasonForShortageSendingActionParent(declaration);
			return new ReasonForShortageMessageSendingForm(messageSendingObjectParent);
		}

		class ReasonForShortageMessageSendingForm_ForTest : ReasonForShortageMessageSendingForm
		{
			public ReasonForShortageMessageSendingForm_ForTest(ReasonForShortageSendingActionParent parent) : base(parent)
			{
			}

			new public KSplitContainer WarningSplitContainer => base.WarningSplitContainer;
		}
	}
}
