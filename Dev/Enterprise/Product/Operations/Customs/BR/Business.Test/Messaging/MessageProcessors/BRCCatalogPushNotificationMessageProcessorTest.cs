using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Customs.BR.MessageDefinitions.PushNotification;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCCatalogPushNotificationMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCCatalogPushNotificationMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "PUS" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "CAT" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCCatalogPushNotificationMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "Goods Catalog Push Notification Response", processor.MessageFriendlyName);
		}

		[TestDate(2023, 2, 1)]
		public void TestProcessMessage_UpdateGoodsCatalogByTariff()
		{
			var org1 = CreateOrgHeader("25043511000111");
			var org2 = CreateOrgHeader("03142520000127");

			var catalog1 = CreateGoodsCatalog("0001", "09022000", org1.PK);
			var catalog2 = CreateGoodsCatalog("0002", "09022000", org1.PK);
			var catalog3 = CreateGoodsCatalog("0003", "09022000", org2.PK);
			var catalog4 = CreateGoodsCatalog("0004", "09022111", org1.PK);

			Factory.Save();

			var ediMessage = CreateMessage(GetMessageText("25043511000111", "09022000"));
			ediMessage.Interchange.EI_SystemCreateTimeUtc = new ZDateTime(2023, 1, 1);

			var logger = ExecuteMessageProcessor(ediMessage);
			AssertEquals("UpdateCustomStatusOnSaving Suspended", true, catalog1.IsUpdateCustomStatusOnSavingSuspended);
			AssertEquals("UpdateCustomStatusOnSaving Suspended", true, catalog2.IsUpdateCustomStatusOnSavingSuspended);

			Factory.Save();

			AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, catalog1.IsUpdateCustomStatusOnSavingSuspended);
			AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, catalog2.IsUpdateCustomStatusOnSavingSuspended);

			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, ediMessage.EM_Status);
			AssertEquals("CGC_AuthorityStatus updated", "1", catalog1.CGC_AuthorityStatus);
			AssertEquals("CGC_AuthorityStatus updated", "1", catalog2.CGC_AuthorityStatus);
			AssertEquals("CGC_AuthorityStatus NOT updated due to Owner not match", ZString.Empty, catalog3.CGC_AuthorityStatus);
			AssertEquals("CGC_AuthorityStatus NOT updated due to Tariff not match", ZString.Empty, catalog4.CGC_AuthorityStatus);

			AssertCustomsUpdateEventAdded(catalog1, new ZDateTime(2022, 12, 31, 22, 0, 0), "|DES=Produtos desativados no catálogo da empresa 25043511000111|PRD=09022000|RES=Os produtos catalogados para o código NCM 02011000 foram desativados pelo gestor do sistema em função da criação de novos atributos obrigatórios. Antes de utilizar esses produtos nas operações, será necessária a geração de novas versões para preenchimento dos novos atributos.");
			AssertCustomsUpdateEventAdded(catalog2, new ZDateTime(2022, 12, 31, 22, 0, 0), "|DES=Produtos desativados no catálogo da empresa 25043511000111|PRD=09022000|RES=Os produtos catalogados para o código NCM 02011000 foram desativados pelo gestor do sistema em função da criação de novos atributos obrigatórios. Antes de utilizar esses produtos nas operações, será necessária a geração de novas versões para preenchimento dos novos atributos.");
			AssertCustomsUpdateEventNotAdded(catalog3);
			AssertCustomsUpdateEventNotAdded(catalog4);
		}

		[TestDate(2023, 2, 1)]
		public void TestProcessMessage_UpdateGoodsCatalogByProductCodes()
		{
			var org1 = CreateOrgHeader("25043511000111");
			var org2 = CreateOrgHeader("03142520000127");

			var catalog1 = CreateGoodsCatalog("0001", "09022000", org1.PK);
			var catalog2 = CreateGoodsCatalog("0002", "09022000", org1.PK);
			var catalog3 = CreateGoodsCatalog("0003", "09022000", org2.PK);
			var catalog4 = CreateGoodsCatalog("0004", "09022111", org1.PK);

			Factory.Save();

			var ediMessage = CreateMessage(GetMessageText("25043511000111", tariff: null, productCodes: new[] { "0001", "0002", "0003" }));

			var logger = ExecuteMessageProcessor(ediMessage);
			Factory.Save();
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, ediMessage.EM_Status);
			AssertEquals("CGC_AuthorityStatus updated", "1", catalog1.CGC_AuthorityStatus);
			AssertEquals("CGC_AuthorityStatus updated", "1", catalog2.CGC_AuthorityStatus);
			AssertEquals("CGC_AuthorityStatus NOT updated due to Owner not match", ZString.Empty, catalog3.CGC_AuthorityStatus);
			AssertEquals("CGC_AuthorityStatus NOT updated due to Product Code not match", ZString.Empty, catalog4.CGC_AuthorityStatus);

			AssertCustomsUpdateEventAdded(catalog1, new ZDateTime(2023, 2, 1), "|DES=Produtos desativados no catálogo da empresa 25043511000111|PRD=0001|RES=Os produtos catalogados para o código NCM 02011000 foram desativados pelo gestor do sistema em função da criação de novos atributos obrigatórios. Antes de utilizar esses produtos nas operações, será necessária a geração de novas versões para preenchimento dos novos atributos.");
			AssertCustomsUpdateEventAdded(catalog2, new ZDateTime(2023, 2, 1), "|DES=Produtos desativados no catálogo da empresa 25043511000111|PRD=0002|RES=Os produtos catalogados para o código NCM 02011000 foram desativados pelo gestor do sistema em função da criação de novos atributos obrigatórios. Antes de utilizar esses produtos nas operações, será necessária a geração de novas versões para preenchimento dos novos atributos.");
			AssertCustomsUpdateEventNotAdded(catalog3);
			AssertCustomsUpdateEventNotAdded(catalog4);
		}

		public void TestProcessMessage_InvalidResponse()
		{
			AssertProcessInvalidResponse(string.Empty);
			AssertProcessInvalidResponse(GetMessageText(rootCpfCnpj: null));
			AssertProcessInvalidResponse(GetMessageText(tariff: null, productCodes: null));

			void AssertProcessInvalidResponse(string messageText)
			{
				var ediMessage = CreateMessage(messageText);

				var logger = ExecuteMessageProcessor(ediMessage);
				CombineAssertions(() =>
				{
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, ediMessage.EM_Status);
					AssertContains("Logger", "Error: \tMessage #1: Message deserialization was failed.", logger.LogMessages.ToString());
				});
			}
		}

		public void TestProcessMessage_CnpjCpfNotFound()
		{
			var ediMessage = CreateMessage(GetMessageText());

			var logger = ExecuteMessageProcessor(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, ediMessage.EM_Status);
				AssertContains("Logger", "Error: \tMessage #1: CNPJ or CPF 25043511000111 not found.", logger.LogMessages.ToString());
			});
		}

		public void TestProcessMessage_CnpjCpfEmpty()
		{
			var ediMessage = CreateMessage(GetMessageText(rootCpfCnpj: ""));

			var logger = ExecuteMessageProcessor(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, ediMessage.EM_Status);
				AssertContains("Logger", "Error: \tMessage #1: CNPJ or CPF is empty.", logger.LogMessages.ToString());
			});
		}

		void AssertCustomsUpdateEventAdded(CusGoodsCatalog catalog, ZDateTime expectedEventTime, string expectedReference)
		{
			CombineAssertions(() =>
			{
				var log = catalog.Logs.MostRecentLogByEventTime(Events.CustomsUpdate);
				AssertNotNull("Customs Update Event added", log);
				AssertEquals("SL_Reference", expectedReference, log.SL_Reference);
				AssertEquals("SL_EventTime", expectedEventTime, log.SL_EventTime);
			});
		}

		void AssertCustomsUpdateEventNotAdded(CusGoodsCatalog catalog)
		{
			AssertNull("Customs Update Event NOT added", catalog.Logs.MostRecentLogByEventTime(Events.CustomsUpdate));
		}

		EDIMessage CreateMessage(ZString catalogMessage)
		{
			var interchange = Factory.New<BREDIInterchange>();
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_InterchangeType = MessageTypeList.Codes.PUS;
			interchange.EI_From = "BRCustoms";
			interchange.EI_To = "BRCustoms";

			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_MessageNum = "1";
			message.EM_MessageType = MessageTypeList.Codes.PUS;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.GoodsCatalog;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_EI = interchange.PK;
			message.EM_MessageText = catalogMessage;

			return message;
		}

		CusGoodsCatalog CreateGoodsCatalog(ZString authorityIdentifier, ZString tariff, ZGuid ownerPK)
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_CatalogCode = authorityIdentifier;
			goodsCatalog.CGC_AuthorityIdentifier = authorityIdentifier;
			goodsCatalog.CGC_Description = "description " + authorityIdentifier;
			goodsCatalog.CGC_Type = "BTH";
			goodsCatalog.CGC_Tariff = tariff;
			goodsCatalog.CGC_OH_Owner = ownerPK;
			return goodsCatalog;
		}

		OrgHeader CreateOrgHeader(ZString rootCpfCnpj)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, rootCpfCnpj, Core.Constants.CountryCodes.Brazil);
			return org;
		}

		internal static string GetMessageText(string rootCpfCnpj = "25043511000111", string tariff = "09022000", string[] productCodes = null)
		{
			return JsonSerializer.Serialize(new ProductCatalogEvent()
			{
				titulo = "Produtos desativados no catálogo da empresa 25043511000111",
				mensagem = "Os produtos catalogados para o código NCM 02011000 foram desativados pelo gestor do sistema em função da criação de novos atributos obrigatórios. Antes de utilizar esses produtos nas operações, será necessária a geração de novas versões para preenchimento dos novos atributos.",
				cpfCnpjRaiz = rootCpfCnpj,
				ncm = tariff,
				codigosProduto = productCodes?.ToList()
			});
		}
	}
}
