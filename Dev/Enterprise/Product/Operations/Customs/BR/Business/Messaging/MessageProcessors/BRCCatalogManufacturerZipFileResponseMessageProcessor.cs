using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogManufacturerZipFileResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCCatalogManufacturerZipFileResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
		protected override string MessageFriendlyNameCore => Res.GetString("3B8CD84A-E9FF-4F17-89C1-16154DC43730", "Goods Catalog Manufacturer ZIP File Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CAT };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.ManufacturerZipFile };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			CusGoodsCatalog goodsCatalog = null;

			var manufacturerIntegration = JsonSerializer.Deserialize<FabricanteIntegracaoDTO>(message.EM_MessageText);
			if (manufacturerIntegration != null && manufacturerIntegration.codigoProduto.HasValue && !manufacturerIntegration.cpfCnpjRaiz.IsNullOrEmpty() && !manufacturerIntegration.codigoPais.IsNullOrEmpty())
			{
				if (BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(message.Factory,
					BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(message, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.CatalogZipFile, EDIMessageSubTypeList.Codes.OperatorZipFile })))
				{
					var identifier = manufacturerIntegration.codigoProduto.ToString();
					goodsCatalog = new CusGoodsCatalog.Loader(message.Factory).GetGoodsCatalogByAuthorityIdentifierAndOwner(identifier, manufacturerIntegration.cpfCnpjRaiz);
					if (goodsCatalog == null)
					{
						Logger.LogError($"Message #{message.EM_MessageNum}: No matching Catalog was found with Owner Root CNPJ '{manufacturerIntegration.cpfCnpjRaiz}' and Lookup Code '{identifier}'.");
					}
				}
				else
				{
					var warningMessage = $"Message #{message.EM_MessageNum} postponed, the response for outgoing Catalog Zip File or Operator Zip File wasn't processed yet.";
					Logger.LogWarning(warningMessage);
					throw new MessageProcessLockException(warningMessage);
				}
			}
			else
			{
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed or tag '{nameof(manufacturerIntegration.codigoProduto)}', '{nameof(manufacturerIntegration.codigoPais)}'  or '{nameof(manufacturerIntegration.cpfCnpjRaiz)}' not found.");
			}

			return goodsCatalog;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusGoodsCatalog goodsCatalog)
			{
				goodsCatalog.SuspendUpdateCustomStatusOnSavingUntilSaved();

				var manufacturerIntegration = JsonSerializer.Deserialize<FabricanteIntegracaoDTO>(message.EM_MessageText);
				var countryCode = manufacturerIntegration.codigoPais;
				var authorityCode = manufacturerIntegration.codigoOperadorEstrangeiro ?? string.Empty;

				if (!goodsCatalog.ForeignOperators.Any(c => c.CountryCode == countryCode && c.AuthorityCode == authorityCode))
				{
					var cusBRForeignOperator = new CusBRForeignOperator.Loader(message.Factory).LoadByOwnerAndAuthorityIdentifier(goodsCatalog.Owner, authorityCode);
					var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
					foreignOperator.CountryCode = countryCode;
					foreignOperator.AuthorityCode = authorityCode;
					foreignOperator.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;

					if (cusBRForeignOperator != null)
					{
						foreignOperator.CGI_BFR_ForeignOperator = cusBRForeignOperator.PK;
					}
				}
			}
		}

		protected override void SetHeldUntilDate(EDIMessage message, IEnumerable<EDIMessage> unprocessedMessages)
		{
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(2);
		}
	}
}
