using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	class GoodsCatalogDownloadMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendMessagesAndSave_DownloadCatalogAndDownloadForeignOperator()
		{
			var (downloadObject, password, owner) = NewGoodsCatalogDownloadObject();
			downloadObject.DownloadCatalog = true;
			downloadObject.DownloadForeignOperator = true;

			CombineAssertions("EDIMessages generated", () =>
			{
				AssertEquals("3 new messages created", 3, new GoodsCatalogDownloadMessageSender(downloadObject).SendMessagesAndSave());

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms));
				AssertEquals("3 messages created", 3, messages.Length);
				AssertEDIMessage(messages, EDIMessageSubTypeList.Codes.CatalogZipFile, password.PK, owner.PK);
				AssertEDIMessage(messages, EDIMessageSubTypeList.Codes.ManufacturerZipFile, password.PK, owner.PK);
				AssertEDIMessage(messages, EDIMessageSubTypeList.Codes.OperatorZipFile, password.PK, owner.PK);

				AssertEquals("CWS event added on Owner", Events.DownloadCatalogRequestSent.Code, owner.Logs.MostRecentLogByPostedDate.SL_SE_NKEvent);
			});
		}

		public void TestSendMessagesAndSave_DownloadCatalog()
		{
			var (downloadObject, password, owner) = NewGoodsCatalogDownloadObject();
			downloadObject.DownloadCatalog = true;
			downloadObject.DownloadForeignOperator = false;

			CombineAssertions("EDIMessages generated", () =>
			{
				AssertEquals("2 new messages created", 2, new GoodsCatalogDownloadMessageSender(downloadObject).SendMessagesAndSave());

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms));
				AssertEquals("2 messages created", 2, messages.Length);
				AssertEDIMessage(messages, EDIMessageSubTypeList.Codes.CatalogZipFile, password.PK, owner.PK);
				AssertEDIMessage(messages, EDIMessageSubTypeList.Codes.ManufacturerZipFile, password.PK, owner.PK);

				AssertEquals("CWS event added on Owner", Events.DownloadCatalogRequestSent.Code, owner.Logs.MostRecentLogByPostedDate.SL_SE_NKEvent);
			});
		}

		public void TestSendMessagesAndSave_DownloadForeignOperator()
		{
			var (downloadObject, password, owner) = NewGoodsCatalogDownloadObject();
			downloadObject.DownloadCatalog = false;
			downloadObject.DownloadForeignOperator = true;

			CombineAssertions("EDIMessage generated", () =>
			{
				AssertEquals("1 new message created", 1, new GoodsCatalogDownloadMessageSender(downloadObject).SendMessagesAndSave());

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms));
				AssertEquals("1 message created", 1, messages.Length);
				AssertEDIMessage(messages, EDIMessageSubTypeList.Codes.OperatorZipFile, password.PK, owner.PK);

				AssertNotEquals("CWS event NOT added on Owner", Events.DownloadCatalogRequestSent.Code, owner.Logs.MostRecentLog.SL_SE_NKEvent);
			});
		}

		public void TestSendMessagesAndSave_HandleSaveException()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_OH_Owner = ZGuid.Invalid;

			var goods = new GoodsCatalogDownloadObject(Factory);
			goods.DownloadCatalog = true;
			goods.DownloadForeignOperator = true;
			goods.OwnerCode = "123455";
			new GoodsCatalogDownloadMessageSender(goods).SendMessagesAndSave();

			var query = new ZQuery(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.CAT);
			var messages = Factory.Load<EDIMessage>(query);

			AssertEquals(0, messages.Length);
		}

		public void TestCanSendMessage()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "XVBQP68S";
			orgHeader.OH_FullName = "TEST COMPANY";
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "58.500.398/-04", Core.Constants.CountryCodes.Brazil);
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "58500398", Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			CombineAssertions(() =>
			{
				var downloadObject = new GoodsCatalogDownloadObject(Factory);
				var messageSender = new GoodsCatalogDownloadMessageSender(downloadObject);
				AssertEquals("Owner Root CNPJ is empty", false, messageSender.CanSendMessage());

				downloadObject.OwnerCode = "XVBQP68S";
				AssertEquals("No request sent", true, messageSender.CanSendMessage());

				var czpRequestMessage = CreateOutgoingMessage(EDIMessageSubTypeList.Codes.CatalogZipFile);
				var mziRequestMessage = CreateOutgoingMessage(EDIMessageSubTypeList.Codes.ManufacturerZipFile);
				var oziRequestMessage = CreateOutgoingMessage(EDIMessageSubTypeList.Codes.OperatorZipFile);
				Factory.Save();

				AssertEquals("Download Catalog request Messages in queue", false, messageSender.CanSendMessage());

				var czpRequestInterchange = CreateOutgoingInterchange(czpRequestMessage);
				var mziRequestInterchange = CreateOutgoingInterchange(mziRequestMessage);
				var oziRequestInterchange = CreateOutgoingInterchange(oziRequestMessage);
				Factory.Save();
				AssertEquals("Download Catalog request Messages sent and Interchanges in queue", false, messageSender.CanSendMessage());

				var czpResponseInterchange = CreateIncomingInterchange(czpRequestInterchange);
				var mziResponseInterchange = CreateIncomingInterchange(mziRequestInterchange);
				var oziResponseInterchange = CreateIncomingInterchange(oziRequestInterchange);
				Factory.Save();
				AssertEquals("Download Catalog response Interchanges in queue", false, messageSender.CanSendMessage());

				var czpResponseMessage = CreateIncomingMessage(czpResponseInterchange);
				var mziResponseMessage = CreateIncomingMessage(mziResponseInterchange);
				var oziResponseMessage = CreateIncomingMessage(oziResponseInterchange);
				Factory.Save();
				AssertEquals("Download Catalog response Messages in queue", false, messageSender.CanSendMessage());

				czpResponseMessage.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				AssertEquals("CZP Download Catalog response Messages processed", false, messageSender.CanSendMessage());

				mziResponseMessage.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				AssertEquals("MZI Download Catalog response Messages processed", false, messageSender.CanSendMessage());

				oziResponseMessage.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				AssertEquals("OZI Download Catalog response Messages processed", true, messageSender.CanSendMessage());

				CreateOutgoingMessage(EDIMessageSubTypeList.Codes.CatalogZipFile).EM_ApplicationReference = "12345678";
				Factory.Save();
				AssertEquals("CZP Download Catalog request Message for another Owner in queue", true, messageSender.CanSendMessage());

				CreateOutgoingMessage(EDIMessageSubTypeList.Codes.CatalogZipFile);
				Factory.Save();
				AssertEquals("Another CZP Download Catalog request Message in queue", false, messageSender.CanSendMessage());
			});

			EDIMessage CreateOutgoingMessage(string messageSubType)
			{
				var message = BRCResponseMessageProcessorTest.CreateMessage(Factory,
					ZGuid.Empty, orgHeader, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, MessageTypeList.Codes.CAT, messageSubType);
				message.EM_ApplicationReference = "58500398";
				return message;
			}

			EDIInterchange CreateOutgoingInterchange(EDIMessage message)
			{
				var interchange = BRCResponseMessageProcessorTest.CreateInterchange(Factory,
					ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchange.Status.Queued, MessageTypeList.Codes.CAT);
				message.EM_EI = interchange.PK;
				message.EM_Status = EDIMessage.Status.Sent;
				return interchange;
			}

			EDIInterchange CreateIncomingInterchange(EDIInterchange outgoingInterchange)
			{
				outgoingInterchange.EI_Status = EDIInterchange.Status.Sent;
				var interchange = BRCResponseMessageProcessorTest.CreateInterchange(Factory,
					outgoingInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, MessageTypeList.Codes.CAT);
				return interchange;
			}

			EDIMessage CreateIncomingMessage(EDIInterchange incomingInterchange)
			{
				incomingInterchange.EI_Status = EDIInterchange.Status.Received;
				return BRCResponseMessageProcessorTest.CreateMessage(Factory,
					incomingInterchange.PK, null, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success);
			}
		}

		(GoodsCatalogDownloadObject, GlbExternalPassword_CCT, OrgHeader) NewGoodsCatalogDownloadObject()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERDOWNLOADCATALOG";
			staff.GS_Code = "DOW";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "XVBQP68S";
			owner.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75.400.331/0001-15", Core.Constants.CountryCodes.Brazil);

			var downloadObject = new GoodsCatalogDownloadObject(Factory);
			downloadObject.OwnerCode = owner.OH_Code;
			downloadObject.BrokerCode = staff.GS_Code;
			downloadObject.DownloadCatalog = true;
			return (downloadObject, password, owner);
		}

		void AssertEDIMessage(EDIMessage[] messages, string expectedSubMessageType, ZGuid certificatePk, ZGuid ownerPK)
		{
			var message = messages.FirstOrDefault(x => x.EM_MessageSubType == expectedSubMessageType);

			Assert("IsInDatabase", message.IsInDatabase);
			AssertNotNull($"{expectedSubMessageType} EDI message added", message);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, message.EM_MessageType);
			AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
			AssertEquals("EM_GP", certificatePk, message.EM_GP);
			AssertEquals("EM_MessageText", ZString.Empty, message.EM_MessageText);
			Assert("IsInDatabase", message.IsInDatabase);

			var applicationReference = "75400331";
			if (expectedSubMessageType == EDIMessageSubTypeList.Codes.CatalogZipFile || expectedSubMessageType == EDIMessageSubTypeList.Codes.OperatorZipFile)
			{
				applicationReference += "|false";
			}
			AssertEquals("EM_ApplicationReference", applicationReference, message.EM_ApplicationReference);
			AssertEquals("EM_LinkTable", "OrgHeader", message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", ownerPK, message.EM_LinkUniqueID);
		}
	}
}
