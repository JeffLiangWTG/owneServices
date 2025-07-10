using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ForeignOperatorMessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			CreateForeignOperatorMessageManager();
			AssertEquals(MessageTypeList.Descriptions.OPE, foreignOperatorMessageManager.MessageFriendlyName);
		}

		public void TestCanSendOriginal()
		{
			CreateForeignOperatorMessageManager();
			AssertEquals(true, foreignOperatorMessageManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			CreateForeignOperatorMessageManager();
			AssertEquals(false, foreignOperatorMessageManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			CreateForeignOperatorMessageManager();
			AssertEquals(false, foreignOperatorMessageManager.IsWaitingForResponse);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals(true, foreignOperatorMessageManager.IsWaitingForResponse);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Failed;
			AssertEquals(false, foreignOperatorMessageManager.IsWaitingForResponse);
		}

		public void TestHasActiveMessages()
		{
			CreateForeignOperatorMessageManager();
			AssertEquals(false, foreignOperatorMessageManager.HasActiveMessages);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals(true, foreignOperatorMessageManager.HasActiveMessages);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.NotSent;
			AssertEquals(false, foreignOperatorMessageManager.HasActiveMessages);
		}

		public void TestGenerateMessages_Activate()
		{
			CreateForeignOperatorMessageManager(ActionList.Codes.Activate);
			AssertGenerateMessages(ActionList.Codes.Activate);
		}

		public void TestGenerateMessages_CreateNewVersion()
		{
			CreateForeignOperatorMessageManager(ActionList.Codes.CreateNewVersion);
			AssertGenerateMessages(ActionList.Codes.CreateNewVersion);
		}

		public void TestGenerateMessages_Deactivate()
		{
			CreateForeignOperatorMessageManager(ActionList.Codes.Deactivate);
			AssertGenerateMessages(ActionList.Codes.Deactivate);
		}

		void AssertGenerateMessages(string action)
		{
			var message = foreignOperatorMessageManager.GenerateMessages()[0];
			var foreignOperator = foreignOperatorMessageManager.ForeignOperator;
			CombineAssertions("EDIMessage generated", () =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.OPE, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", ForeignOperatorMessageTypesList.Codes.ORI, message.EM_MessageSubType);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
				Assert("EM_MessageText", !message.EM_MessageText.IsEmpty);
				AssertEquals("EM_GP", password.PK, message.EM_GP);
				AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
				AssertEquals("EM_LinkTable", "CusBRForeignOperator", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", foreignOperator.PK, message.EM_LinkUniqueID);

				AssertEquals("BFR_MessageCustoms", foreignOperator.BFR_MessageStatus, BRMessageStatusList.Codes.AwaitingResponse);
				var log = foreignOperator.Logs.MostRecentLogByEventTime(Events.MessageSent);
				AssertEquals("SL_Reference", action, log.SL_Reference);
			});
		}

		public void TestSendMessagesAndSave()
		{
			CreateForeignOperatorMessageManager();
			AssertEquals("Two messages sent", 1, sendingObj.SendMessagesAndSave());

			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, sendingObj.ForeignOperator.PK);
			var messages = Factory.Load<EDIMessage>(query);
			var message = messages[0];
			AssertEquals("1 messages generated", 1, messages.Length);
			Assert("1 message packed into one Interchange", messages.AllSame(x => x.EM_EI));
			AssertEquals("EDIMessage saved", true, message.IsInDatabase);
			AssertEquals("Interchange saved", true, message.Interchange.IsInDatabase);
			AssertEquals("EDIMessage created", EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals("Interchange.EI_TransportType", EDIInterchange.TransportType.xT, message.Interchange.EI_TransportType);
			AssertEquals("Interchange.EI_Status", EDIInterchangeStatusList.Codes.Queued, message.Interchange.EI_Status);
		}

		public void TestGenerateMessagesForAmendmentDetection()
		{
			CreateForeignOperatorMessageManager();
			CombineAssertions(() =>
			{
				foreignOperator.BFR_AuthorityVersion = "1";
				foreignOperator.BFR_AuthorityIdentifier = "1";
				AssertEquals("RequiresAmendment should be false when Message Status is not AWA", false, foreignOperatorMessageManager.RequiresAmendment());

				foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
				Factory.Save();

				foreignOperator.BFR_AuthorityVersion = "2";
				AssertEquals("RequiresAmendment should be true when changes made affect OPE-ORI message", true, foreignOperatorMessageManager.RequiresAmendment());
			});
		}

		void CreateForeignOperatorMessageManager(string action = ActionList.Codes.CreateNewVersion)
		{
			newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XXX";

			password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "001";
			manufacturer.OH_FullName = "FOREIGN OPERATOR 1";

			var customsCode = manufacturer.CustomsCodes.AddNew();
			customsCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.TIN;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			customsCode.OK_CustomsRegNo = "50178";

			owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "TEST COMPANY";

			foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer.PK;

			Factory.Save();

			sendingObj = new ForeignOperatorMessageSendingObject(foreignOperator);
			sendingObj.BrokerCode = newStaff.GS_Code;
			sendingObj.Action = action;
			foreignOperatorMessageManager = new ForeignOperatorMessageManager(sendingObj);
		}

		GlbStaff newStaff;
		GlbExternalPassword_CCT password;
		OrgHeader owner;
		ForeignOperatorMessageManager foreignOperatorMessageManager;
		ForeignOperatorMessageSendingObject sendingObj;
		CusBRForeignOperator foreignOperator;
	}
}
