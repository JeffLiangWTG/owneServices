using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogOperatorZipFileResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCCatalogOperatorZipFileResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("1CCDE5D2-6929-4112-A062-8DA23694F753", "Goods Catalog Operator ZIP File Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CAT };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.OperatorZipFile };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			CusBRForeignOperator foreignOperator = null;

			var foreignOperatorDTO = BRMessageHelper.DeserializeObject<OperadorEstrangeiroIntegracaoDTO>(message.EM_MessageText);
			if (foreignOperatorDTO?.cpfCnpjRaiz is string rootCnpj)
			{
				if (message.Factory.FindOrganizationByRootCNPJ(rootCnpj) is OrgHeader owner)
				{
					foreignOperator = new CusBRForeignOperator.Loader(owner.Factory).LoadByOwnerAndAuthorityIdentifier(owner, foreignOperatorDTO.codigo);
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: Organization with RTC '{rootCnpj}' not found.");
				}
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed. Invalid json format.");
			}

			return foreignOperator;
		}

		protected override bool FailIfLinkedObjectNotFound => false;

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var foreignOperatorDTO = BRMessageHelper.DeserializeObject<OperadorEstrangeiroIntegracaoDTO>(message.EM_MessageText);
			if (foreignOperatorDTO != null)
			{
				var identifier = foreignOperatorDTO.codigo;
				var situation = ForeignOperatorCustomsStatusTypeList.MapToCWCode(foreignOperatorDTO.situacao);

				var foreignOperator = message.EM_LinkedObject as CusBRForeignOperator;
				if (foreignOperator == null && message.Factory.FindOrganizationByRootCNPJ(foreignOperatorDTO?.cpfCnpjRaiz) is OrgHeader owner)
				{
					foreignOperator = message.Factory.New<CusBRForeignOperator>();
					foreignOperator.BFR_OH_Owner = owner.PK;
					foreignOperator.BFR_AuthorityIdentifier = identifier;

					message.EM_LinkedObject = foreignOperator;
				}

				if (foreignOperator != null)
				{
					foreignOperator.SuspendUpdateMessageStatusOnSavingUntilSaved();
					if (foreignOperator.BFR_OH_ForeignOperator.IsEmpty)
					{
						foreignOperator.BFR_Name = foreignOperatorDTO?.nome;
						foreignOperator.BFR_RN_NKCountryCode = foreignOperatorDTO?.codigoPais;
					}

					if (foreignOperator.BFR_AuthorityVersion != foreignOperatorDTO.versao)
					{
						foreignOperator.BFR_AuthorityVersion = foreignOperatorDTO.versao;
					}
					if (foreignOperator.BFR_CustomsStatus != situation)
					{
						foreignOperator.BFR_CustomsStatus = situation;
					}

					if (foreignOperator.IsInDatabase)
					{
						Logger.Log(foreignOperator.HasChanges
							? $"Foreign Operator with Authority Identifier '{identifier}' already exists and was updated."
							: $"Foreign Operator with Authority Identifier '{identifier}' already exists and was not updated.");
					}

					foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
				}
			}
		}
	}
}
