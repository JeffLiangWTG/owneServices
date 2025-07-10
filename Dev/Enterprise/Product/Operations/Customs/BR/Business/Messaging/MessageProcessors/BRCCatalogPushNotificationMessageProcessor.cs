using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.PushNotification;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogPushNotificationMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCCatalogPushNotificationMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("871341f1-513c-48c4-9588-fc59364678f5", "Goods Catalog Push Notification Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.PUS };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.GoodsCatalog };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => null;

		protected override bool FailIfLinkedObjectNotFound => false;

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var productCatalogEvent = BRMessageHelper.DeserializeObject<ProductCatalogEvent>(message.EM_MessageText);
			if (productCatalogEvent == null || productCatalogEvent.cpfCnpjRaiz == null || (productCatalogEvent.ncm == null && productCatalogEvent.codigosProduto == null))
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
			}
			else if (string.IsNullOrEmpty(productCatalogEvent.cpfCnpjRaiz))
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				Logger.LogError($"Message #{message.EM_MessageNum}: CNPJ or CPF is empty.");
			}
			else
			{
				var orgHeaderPK = message.Factory.LoadTop1<OrgHeader>(GetOrgHeaderByCNPJQuery(productCatalogEvent.cpfCnpjRaiz))?.PK ?? ZGuid.Empty;

				if (!orgHeaderPK.IsValid)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: CNPJ or CPF {productCatalogEvent.cpfCnpjRaiz} not found.");
				}
				else
				{
					var catalogQuery = ZQuery.NoResultQuery;

					var loadByNcm = !string.IsNullOrEmpty(productCatalogEvent.ncm);
					if (loadByNcm)
					{
						catalogQuery = GetCatalogByTariffQuery(orgHeaderPK, productCatalogEvent.ncm);
					}
					else if (productCatalogEvent.codigosProduto.Count > 0)
					{
						catalogQuery = GetCatalogByAuthorityIdentifierQuery(orgHeaderPK, productCatalogEvent.codigosProduto.ToArray());
					}

					var date = message.Interchange?.EI_SystemCreateTimeUtc.ToLocalBranchTimeOffset() ?? ZDateTimeOffset.Now;

					foreach (var catalog in message.Factory.Load<CusGoodsCatalog>(catalogQuery))
					{
						catalog.SuspendUpdateCustomStatusOnSavingUntilSaved();
						catalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Inactive;
						catalog.AddCustomsUpdateLog(date, productCatalogEvent.titulo, loadByNcm ? catalog.CGC_Tariff : catalog.CGC_AuthorityIdentifier, productCatalogEvent.mensagem);
					}
				}
			}
		}

		ZQuery GetCatalogByTariffQuery(ZGuid orgHeaderPK, ZString tariffCode)
		{
			return new ZQuery(CusGoodsCatalogSchema.CGC_OH_Owner, orgHeaderPK).AddToFilter(CusGoodsCatalogSchema.CGC_Tariff, tariffCode);
		}

		ZQuery GetCatalogByAuthorityIdentifierQuery(ZGuid orgHeaderPK, string[] productCode)
		{
			return new ZQuery(CusGoodsCatalogSchema.CGC_OH_Owner, orgHeaderPK).AddToFilter(CusGoodsCatalogSchema.CGC_AuthorityIdentifier, productCode);
		}

		ZDBOnlyQuery GetOrgHeaderByCNPJQuery(ZString regNo)
		{
			var orgCusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Brazil);
			orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ);
			orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, regNo);

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			query.AddSubQuery(orgCusCodeQuery, JoinCondition.And);

			return query;
		}
	}
}

