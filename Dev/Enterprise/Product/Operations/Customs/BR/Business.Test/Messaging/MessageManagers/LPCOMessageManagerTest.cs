using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class LPCOMessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			CreateLPCOMessageManager();
			AssertEquals(MessageTypeList.Descriptions.CAT, lpcoMessageManager.MessageFriendlyName);
		}

		public void TestCanSendOriginal()
		{
			CreateLPCOMessageManager();
			AssertEquals(true, lpcoMessageManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			CreateLPCOMessageManager();
			AssertEquals(false, lpcoMessageManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			CreateLPCOMessageManager();
			AssertEquals(false, lpcoMessageManager.IsWaitingForResponse);
		}

		public void TestHasActiveMessages()
		{
			CreateLPCOMessageManager();
			AssertEquals(false, lpcoMessageManager.HasActiveMessages);
		}

		public void TestGenerateMessages_ORI()
		{
			CreateLPCOMessageManager();
			lpcoHeader.CPH_RetroactiveDate = new ZDateTimeOffset(2024, 12, 24, 12, 48, 12);
			AssertGenerateMessages(LPCOEntryActionCodeList.Codes.ORI, "{\"dataReferencia\":\"24-12-2024T12:48\"}");
		}

		public void TestGenerateMessages_ALE()
		{
			CreateLPCOMessageManager();
			lpcoMessageSendingObj.NewEffectiveDate = new ZDate(2021, 7, 19);
			lpcoMessageSendingObj.Reason = "reason";
			AssertGenerateMessages(LPCOEntryActionCodeList.Codes.ALE, "{\"novaDataFimVigencia\":\"2021-07-19\",\"justificativa\":\"reason\"}");
		}

		public void TestGenerateMessages_RCA()
		{
			CreateLPCOMessageManager();
			lpcoMessageSendingObj.Reason = "reason";
			AssertGenerateMessages(LPCOEntryActionCodeList.Codes.RCA, "{\"justificativa\":\"reason\"}");
		}

		public void TestGenerateMessages_REQ()
		{
			CreateLPCOMessageManager();
			lpcoMessageSendingObj.Reason = "reason";
			lpcoMessageSendingObj.Requirement = 444;
			AssertGenerateMessages(LPCOEntryActionCodeList.Codes.REQ, "{\"justificativa\":\"reason\"}", "TEST01|444");
		}

		public void TestGenerateMessages_COM()
		{
			CreateLPCOMessageManager();
			lpcoMessageSendingObj.Reason = "reason";
			lpcoMessageSendingObj.PermitNumber = "LPCO1234567890";
			AssertGenerateMessages(LPCOEntryActionCodeList.Codes.COM, "{\"numeroDocumento\":\"LPCO1234567890\",\"versaoCompatibilizacao\":\"\",\"justificativa\":\"reason\"}");
		}

		public void TestGenerateMessages_MSG()
		{
			CreateLPCOMessageManager();
			lpcoMessageSendingObj.Message = "message";
			AssertGenerateMessages(LPCOEntryActionCodeList.Codes.MSG, "{\"mensagem\":\"message\"}");
		}

		void AssertGenerateMessages(ZString messageSubType, string expectedMessageText, string applicationReference = null)
		{
			var password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			lpcoHeader.CPH_MessageStatus = BRMessageStatusList.Codes.NotSent;

			lpcoMessageSendingObj.ShouldSend = true;
			lpcoMessageSendingObj.MessageType = messageSubType;

			var expectedApplicationReference = applicationReference ?? lpcoHeader.CPH_Number;
			var message = lpcoMessageManager.GenerateMessages().First();

			CombineAssertions($"EDIMessage generated for {messageSubType}", () =>
			{
				AssertEquals("Messages count", 1, lpcoHeader.Messages.Count);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.LPC, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", messageSubType, message.EM_MessageSubType);
				AssertEquals("EM_LinkUniqueID", lpcoHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", "CusPermitHeader", message.EM_LinkTable);
				AssertEquals("EM_ApplicationReference", expectedApplicationReference, message.EM_ApplicationReference);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
				AssertEquals("EM_GP", password.PK, message.EM_GP);
				var unformatedMessageText = message.EM_MessageText.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty);
				AssertEquals("EM_MessageText", expectedMessageText, unformatedMessageText);
				AssertEquals(message, lpcoHeader.Messages[0]);
				AssertEquals("CPH_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, lpcoHeader.CPH_MessageStatus);
			});

			lpcoMessageManager.RollbackOnSavingFailed();
			AssertEquals("CPH_MessageStatus", BRMessageStatusList.Codes.NotSent, lpcoHeader.CPH_MessageStatus);
		}

		void CreateLPCOMessageManager()
		{
			newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XXX";

			var password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			lpcoHeader.CPH_Number = "TEST01";

			var lpcoMessageSendingObjectParent = new LPCOMessageSendingObjectParent(lpcoHeader);
			lpcoMessageSendingObjectParent.BrokerCode = newStaff.GS_Code;
			lpcoMessageSendingObj = lpcoMessageSendingObjectParent.SendingObjectsCollection.Cast<LPCOMessageSendingObject>().FirstOrDefault();
			lpcoMessageManager = new LPCOMessageManager(lpcoMessageSendingObj);
		}

		CusLPCOHeader lpcoHeader;
		GlbStaff newStaff;
		LPCOMessageManager lpcoMessageManager;
		LPCOMessageSendingObject lpcoMessageSendingObj;
	}
}
