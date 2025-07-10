using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class MessageMenuExtensionTest : TestCaseWithFactory
	{
		public void TestCheckDeclarationStatusAndLines_DecIsNull()
		{
			CusTempStorageDec storageDec = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CombineAssertions(() =>
			{
				AssertEquals("No Dec", false, storageDec.CheckDeclarationStatusAndLines());
				AssertEquals("Message", "No Declaration has been created.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestCheckDeclarationStatusAndLines_DecAwaitingResponse()
		{
			var storageDec = Factory.New<CUSPRLCusTempStorageDec>();
			storageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CombineAssertions(() =>
			{
				AssertEquals("Dec is sent", false, storageDec.CheckDeclarationStatusAndLines());
				AssertContains("Message", "Cannot send further messages, still awaiting a response.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestCheckDeclarationStatusAndLines_DecHasNoLines()
		{
			var storageDec = Factory.New<CUSPRLCusTempStorageDec>();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CombineAssertions(() =>
			{
				AssertEquals("Dec has no lines", false, storageDec.CheckDeclarationStatusAndLines());
				AssertContains("Message", "Declaration has no lines, message cannot be created.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestCheckDeclarationStatusAndLines_ValidDec()
		{
			var storageDec = Factory.New<CUSPRLCusTempStorageDec>();
			storageDec.CusTempStorageLines.AddNew();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Dec Valid", true, storageDec.CheckDeclarationStatusAndLines());
		}

		public void TestPreSaveMessage()
		{
			using (var menu = new KMenuItem())
			{
				var header = Factory.New<CusTempStorageJobHeader>();
				header.SJH_JobReference = "TEST";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.PreSaveMessage(header);
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
