using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	sealed class ExitControlSendToCustomsMenuCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			CombineAssertions(() =>
			{
				var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreator(Factory.New<CusExitHeader>()).Create();
				AssertEquals("Send to Customs - Text", "Send to Customs", sendToCustomsMenuItem.Text);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var form = (EU.ExitControl.GUI.ExitControlMessageSendingForm)obj;
					AssertType<ExitControlMessageSendingObjectParent>("MessageSendingObjectParent", form.MessageSendingObjectParent);
				});
				sendToCustomsMenuItem.PerformClick();
			});
		}

		public void TestPerformClick()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				var exitReport1 = exitHeader.CusExitReports.AddNew();
				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_LocalReference = "Ref1";
				exitReport1.CER_CXC_Consignment = consignment1.PK;

				var exitReport2 = exitHeader.CusExitReports.AddNew();
				var consignment2 = exitHeader.CusExitConsignments.AddNew();
				consignment2.CXC_LocalReference = "Ref2";
				exitReport2.CER_CXC_Consignment = consignment2.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreatorForTesting(exitHeader).Create();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("2 message(s) sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestPerformClick_NoMessageBuildersForCurrentAESVersion_Version_40()
		{
			var messageVersionRegistry = new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode };
			// AESVersionNumberList.Codes._40 is temporary not in VersionNumbers we have to suspend validation for test, remove it when it's back
			using (messageVersionRegistry.GetValidationSuspender())
			{
				messageVersionRegistry.VersionNumber = AESVersionNumberList.Codes._40;
			}
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { messageVersionRegistry };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var exitHeader = Factory.New<CusExitHeader>();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreator(exitHeader).Create();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Current AES Version is not supported for sending Exit Control messages, please check Registry: Customs -> Country or Region Specific -> Germany -> Customs Message Version.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		sealed class ExitControlSendToCustomsMenuCreatorForTesting : ExitControlSendToCustomsMenuCreator
		{
			public ExitControlSendToCustomsMenuCreatorForTesting(CusExitHeader header) : base(header)
			{
			}

			protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetMessageSendingParent()
			{
				var sendingParent = base.GetMessageSendingParent();
				sendingParent.SendingObjectsCollection.Cast<EU.ExitControl.Business.ExitControlMessageSendingObject>().ForEach(x => x.ShouldSend = true);
				return sendingParent;
			}
		}
	}
}
