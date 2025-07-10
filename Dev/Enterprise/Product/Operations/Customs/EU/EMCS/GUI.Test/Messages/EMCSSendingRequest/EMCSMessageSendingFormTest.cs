using System.Collections;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSMessageSendingForm<EMCSMessageSendingAction>))]
	sealed class EMCSMessageSendingFormTest : ZFormBasherTest
	{
		public void TestPreviewMessageCheckBox_NonDeveloper()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			using (var form = (EMCSMessageSendingForm<EMCSMessageSendingAction>)GetFormToBash())
			{
				form.Show();
				AssertEquals(false, form.PreviewMessageCheckBox.Visible);
			}
		}

		public void TestPreviewMessageCheckBox_Developer()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			using (var form = (EMCSMessageSendingForm<EMCSMessageSendingAction>)GetFormToBash())
			{
				form.Show();
				AssertEquals(true, form.PreviewMessageCheckBox.Visible);
				AssertEquals(true, form.PreviewMessageCheckBox.Enabled);
			}
		}

		public void TestCheckIsOKToSend()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingActionParent = new MinimalSendingActionParent(declaration);
			using (var form = new EMCSMessageSendingFormForTest(sendingActionParent))
			{
				AssertEquals("Configuration.IsOKToSend and base.CheckIsOKToSend are both true", true, form.CheckIsOKToSend);
			}

			var configurationMock = new Mock<IEMCSMessageSendingFormConfiguration>();
			configurationMock.Setup(m => m.IsOKToSend(sendingActionParent)).Returns(false);
			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(configurationMock.Object);
			var configuration = new Hashtable
			{
				{ "Default", objectHandleMock.Object }
			};

			using (ObjectFactory.Substitute("EMCSMessageSendingFormConfigurations", configuration))
			using (var form = new EMCSMessageSendingFormForTest(sendingActionParent))
			{
				AssertEquals("When Configuration.IsOKToSend is false", false, form.CheckIsOKToSend);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<EMCSJobDeclaration>();
			declaration.JE_MessageType = EMCSJobDeclaration.EMCSMessageTypeCode;
			var messageSendingObjectParent = new MinimalSendingActionParent(declaration);
			return new EMCSMessageSendingForm<EMCSMessageSendingAction>(messageSendingObjectParent);
		}

		class EMCSMessageSendingFormForTest : EMCSMessageSendingForm<EMCSMessageSendingAction>
		{
			public EMCSMessageSendingFormForTest(EMCSMessageSendingActionParent<EMCSMessageSendingAction> parent) : base(parent)
			{
			}

			public new bool CheckIsOKToSend => CheckIsOKToSend();
		}
	}
}
