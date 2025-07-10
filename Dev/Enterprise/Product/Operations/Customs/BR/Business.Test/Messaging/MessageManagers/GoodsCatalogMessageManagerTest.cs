using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class GoodsCatalogMessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			CreateCatalogMessageManager();
			AssertEquals(MessageTypeList.Descriptions.CAT, catalogMessageManager.MessageFriendlyName);
		}

		public void TestCanSendOriginal()
		{
			CreateCatalogMessageManager();
			AssertEquals(true, catalogMessageManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			CreateCatalogMessageManager();
			AssertEquals(false, catalogMessageManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			CreateCatalogMessageManager();
			AssertEquals(false, catalogMessageManager.IsWaitingForResponse);
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals(true, catalogMessageManager.IsWaitingForResponse);
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Failed;
			AssertEquals(false, catalogMessageManager.IsWaitingForResponse);
		}

		public void TestHasActiveMessages()
		{
			CreateCatalogMessageManager();

			AssertEquals(true, catalogMessageManager.HasActiveMessages);
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals(true, catalogMessageManager.HasActiveMessages);

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;
			AssertEquals(true, catalogMessageManager.HasActiveMessages);

			goodsCatalog.CGC_AuthorityIdentifier = "";
			AssertEquals(false, catalogMessageManager.HasActiveMessages);
		}

		[TestDate(2021, 12, 31, 12, 00, 00)]
		public void TestGenerateMessages_OnlyORI()
		{
			CreateCatalogMessageManager();

			goodsCatalog.CGC_AuthorityIdentifier = "";
			goodsCatalog.CGC_MessageStatus = EDIMessageSubTypeList.Codes.Cancel;

			var messages = catalogMessageManager.GenerateMessages();
			var message = messages[0];

			AssertGenerateMessages(message, EDIMessageSubTypeList.Codes.Original, expectedMessageText.Replace("\r\n  \"codigo\": 8,", string.Empty));

			catalogMessageManager.RollbackOnSavingFailed();
			var logAfterDelete = goodsCatalog.Logs.MostRecentLogByEventTime(Events.MessageSent);
			AssertEquals("CGC_MessageStatus", EDIMessageSubTypeList.Codes.Cancel, goodsCatalog.CGC_MessageStatus);
			AssertNull("Log should be deleted", logAfterDelete);
		}

		[TestDate(2021, 12, 31, 12, 00, 00)]
		public void TestGenerateMessages_OnlyLIN()
		{
			CreateCatalogMessageManager(action: ActionList.Codes.LinkUnlinkForeignOperator);
			goodsCatalog.CGC_MessageStatus = EDIMessageSubTypeList.Codes.Cancel;
			goodsCatalog.ForeignOperators[0].CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;
			goodsCatalog.ForeignOperators[1].CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;

			var messages = catalogMessageManager.GenerateMessages();
			var message = messages[0];

			AssertGenerateMessages(message, EDIMessageSubTypeList.Codes.Link, expectedLINMessageText, action: ActionList.Codes.LinkUnlinkForeignOperator);

			catalogMessageManager.RollbackOnSavingFailed();
			var logAfterDelete = goodsCatalog.Logs.MostRecentLogByEventTime(Events.MessageSent);
			AssertEquals("CGC_MessageStatus", EDIMessageSubTypeList.Codes.Cancel, goodsCatalog.CGC_MessageStatus);
			AssertNull("Log should be deleted", logAfterDelete);
		}

		[TestDate(2021, 12, 31, 12, 00, 00)]
		public void TestGenerateMessages_ORI_LIN_ExceedMaxNumberOfRowsInProductCatalogMessage()
		{
			CreateCatalogMessageManager();

			var expectedMessageTextLIN1 = @"[
  {
    ""cpfCnpjRaiz"": """",
    ""codigoOperadorEstrangeiro"": ""OPE_1"",
    ""cpfCnpjFabricante"": """",
    ""conhecido"": true,
    ""codigoProduto"": 8,
    ""vincular"": true,
    ""codigoPais"": ""BR""
  }
]";
			var expectedMessageTextLIN2 = @"[
  {
    ""cpfCnpjRaiz"": """",
    ""cpfCnpjFabricante"": """",
    ""conhecido"": false,
    ""codigoProduto"": 8,
    ""vincular"": false,
    ""codigoPais"": ""UY""
  }
]";
			using (BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				goodsCatalog.CGC_MessageStatus = EDIMessageSubTypeList.Codes.Cancel;
				goodsCatalog.ForeignOperators[0].CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;
				goodsCatalog.ForeignOperators[1].CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;

				var messages = catalogMessageManager.GenerateMessages();
				AssertGenerateMessages(messages[0], EDIMessageSubTypeList.Codes.Original, expectedMessageText);
				AssertGenerateMessages(messages[1], EDIMessageSubTypeList.Codes.Link, expectedMessageTextLIN1);
				AssertGenerateMessages(messages[2], EDIMessageSubTypeList.Codes.Link, expectedMessageTextLIN2);

				catalogMessageManager.RollbackOnSavingFailed();
				AssertEquals("CGC_MessageStatus", EDIMessageSubTypeList.Codes.Cancel, goodsCatalog.CGC_MessageStatus);
			}
		}

		[TestDate(2021, 12, 31, 12, 00, 00)]
		public void TestGenerateMessages_ORI_LIN_NotExceedMaxNumberOfRowsInProductCatalogMessage()
		{
			CreateCatalogMessageManager();

			using (BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				goodsCatalog.CGC_MessageStatus = EDIMessageSubTypeList.Codes.Cancel;
				goodsCatalog.ForeignOperators[0].CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;
				goodsCatalog.ForeignOperators[1].CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;

				var messages = catalogMessageManager.GenerateMessages();
				AssertGenerateMessages(messages[0], EDIMessageSubTypeList.Codes.Original, expectedMessageText);
				AssertGenerateMessages(messages[1], EDIMessageSubTypeList.Codes.Link, expectedLINMessageText);

				catalogMessageManager.RollbackOnSavingFailed();
				AssertEquals("CGC_MessageStatus", EDIMessageSubTypeList.Codes.Cancel, goodsCatalog.CGC_MessageStatus);
			}
		}

		void AssertGenerateMessages(EDIMessage message, string expectedMessageSubType, string expectedMessageText, string action = ActionList.Codes.CreateDraft)
		{
			CombineAssertions("EDIMessage generated", () =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
				AssertEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
				AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
				AssertEquals("EM_GP", password.PK, message.EM_GP);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, message.EM_LinkUniqueID);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);

				var log = goodsCatalog.Logs.MostRecentLogByEventTime(Events.MessageSent);
				AssertEquals("SL_Reference", action, log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2021, 12, 31, 12, 00, 00), log.SL_EventTime);
			});
		}

		public void TestGenerateMessagesForAmendmentDetection()
		{
			CreateCatalogMessageManager();

			var productionInfo = goodsCatalog.ForeignOperators.AddNew();
			productionInfo.CountryCode = "CL";
			productionInfo.AuthorityCode = "OPE_2";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("RequiresAmendment should be false when Message Status is not AWA", false, catalogMessageManager.RequiresAmendment());

				goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
				Factory.Save();

				goodsCatalog.CGC_AuthorityIdentifier = "20";
				AssertEquals("RequiresAmendment should be true when changes made affect CAT-ORI message", true, catalogMessageManager.RequiresAmendment());
				Factory.Save();

				goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Active;
				AssertEquals("RequiresAmendment should be false when changes not affect CAT-ORI message", false, catalogMessageManager.RequiresAmendment());
				Factory.Save();

				productionInfo.CountryCode = "CA";
				AssertEquals("RequiresAmendment should be true when changes made affect CAT-LIN message", true, catalogMessageManager.RequiresAmendment());
				Factory.Save();

				goodsCatalog.ForeignOperators.AddNew().CountryCode = "CN";
				AssertEquals("RequiresAmendment should be true when add new ProductionInfo", true, catalogMessageManager.RequiresAmendment());
				Factory.Save();

				productionInfo.CGI_CustomsStatus = BRMessageStatusList.Codes.Accepted;
				AssertEquals("RequiresAmendment should be true when changes not affect CAT-LIN message", true, catalogMessageManager.RequiresAmendment());

				productionInfo.Delete();
				AssertEquals("RequiresAmendment should be true when delete ProductionInfo", true, catalogMessageManager.RequiresAmendment());
			});
		}

		void CreateCatalogMessageManager(string authorityIdentifier = "8", string action = ActionList.Codes.CreateDraft)
		{
			newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XXX";

			password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_CatalogCode = "C123";
			goodsCatalog.CGC_AuthorityIdentifier = authorityIdentifier;
			goodsCatalog.CGC_AuthorityVersion = "9";

			var productionInfo = goodsCatalog.ForeignOperators.AddNew();
			productionInfo.CGI_SystemCreateTimeUtc = ZDateTime.Now;
			productionInfo.CountryCode = "BR";
			productionInfo.AuthorityCode = "OPE_1";

			productionInfo = goodsCatalog.ForeignOperators.AddNew();
			productionInfo.CGI_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			productionInfo.CountryCode = "UY";

			catalogMessageSending = new GoodsCatalogMessageSendingObject(goodsCatalog);
			catalogMessageSending.BrokerCode = newStaff.GS_Code;
			catalogMessageSending.Action = action;
			catalogMessageManager = new GoodsCatalogMessageManager(catalogMessageSending);
		}

		GlbStaff newStaff;
		GlbExternalPassword_CCT password;
		CusGoodsCatalog goodsCatalog;
		GoodsCatalogMessageManager catalogMessageManager;
		GoodsCatalogMessageSendingObject catalogMessageSending;

		readonly string expectedMessageText = @"{
  ""seq"": 1,
  ""codigo"": 8,
  ""descricao"": """",
  ""denominacao"": ""description"",
  ""cpfCnpjRaiz"": """",
  ""situacao"": ""RASCUNHO"",
  ""modalidade"": ""IMPORTACAO"",
  ""ncm"": """",
  ""atributos"": [],
  ""atributosMultivalorados"": [],
  ""atributosCompostos"": [],
  ""codigosInterno"": []
}";

		readonly string expectedLINMessageText = @"[
  {
    ""cpfCnpjRaiz"": """",
    ""codigoOperadorEstrangeiro"": ""OPE_1"",
    ""cpfCnpjFabricante"": """",
    ""conhecido"": true,
    ""codigoProduto"": 8,
    ""vincular"": true,
    ""codigoPais"": ""BR""
  },
  {
    ""cpfCnpjRaiz"": """",
    ""cpfCnpjFabricante"": """",
    ""conhecido"": false,
    ""codigoProduto"": 8,
    ""vincular"": false,
    ""codigoPais"": ""UY""
  }
]";
	}
}
