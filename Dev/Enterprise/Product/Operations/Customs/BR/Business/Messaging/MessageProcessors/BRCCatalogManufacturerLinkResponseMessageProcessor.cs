using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogManufacturerLinkResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCCatalogManufacturerLinkResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("087A709F-453A-4D9F-A870-3A24FA6BA9A0", "Goods Catalog Manufacturer Link Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CAT };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Link };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			CusGoodsCatalog goodsCatalog = null;

			if (ParseReponseAndFindRequest(message).Request is FabricanteIntegracaoDTO request)
			{
				var owner = message.Factory.FindOrganizationByRootCNPJ(request.cpfCnpjRaiz);
				if (owner != null)
				{
					goodsCatalog = new CusGoodsCatalog.Loader(message.Factory).GetGoodsCatalogByAuthorityIdentifierAndOwner(request.codigoProduto?.ToString(), owner.PK);

					if (goodsCatalog == null)
					{
						Logger.LogError($"Unable to find a Goods Catalog with Authority Identifier '{request.codigoProduto}' and Root CNPJ '{request.cpfCnpjRaiz}' for {message.EM_MessageType} message #{message.EM_MessageNum}. The message status was updated to 'FAL'.");
					}
				}
				else
				{
					Logger.LogError($"Root CNPJ '{request.cpfCnpjRaiz}' does not have a matching organization for {message.EM_MessageType} message #{message.EM_MessageNum}.The message status was updated to 'FAL'.");
				}
			}

			return goodsCatalog;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusGoodsCatalog goodsCatalog)
			{
				goodsCatalog.SuspendUpdateCustomStatusOnSavingUntilSaved();

				var (request, response) = ParseReponseAndFindRequest(message);
				if (request != null && response != null)
				{
					var countryCode = request.codigoPais;
					var authorityCode = request.conhecido.GetValueOrDefault() ? request.codigoOperadorEstrangeiro : string.Empty;

					var productionInfo = goodsCatalog.ForeignOperators.FirstOrDefault(x => x.CountryCode == countryCode && x.AuthorityCode == authorityCode);
					if (productionInfo == null)
					{
						message.EM_Status = EDIMessageStatusList.Codes.Failed;
						Logger.LogError($"Unable to find a Production Info with Reference '{countryCode}', Authority Code '{authorityCode}'");
					}
					else
					{
						if (response.sucesso)
						{
							if (request.vincular.GetValueOrDefault())
							{
								productionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
							}
							else
							{
								productionInfo.Delete();
							}
						}
						message.EM_Status = EDIMessageStatusList.Codes.Received;
					}

					var messagesSentAtTheSameTime = BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(message,
							MessageTypeList.Codes.CAT, [EDIMessageSubTypeList.Codes.Original, EDIMessageSubTypeList.Codes.Link]);

					if (!messagesSentAtTheSameTime.Any(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.Original) &&
						BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(message.Factory, messagesSentAtTheSameTime))
					{
						BRCCatalogSuccessResponseMessageProcessor.UpdateMessageStatus(goodsCatalog);
					}
				}
			}
		}

		(FabricanteIntegracaoDTO Request, LoteValidacaoVersaoDTO Response) ParseReponseAndFindRequest(EDIMessage message)
		{
			FabricanteIntegracaoDTO request = null;
			LoteValidacaoVersaoDTO response = null;

			var incomingInterchange = message.Interchange;
			if (incomingInterchange == null)
			{
				Logger.LogError("Unable to find the incoming Interchange.");
			}
			else
			{
				var outgoingInterchange = BRMessageHelper.GetOutgoingInterchange(incomingInterchange);
				if (outgoingInterchange == null)
				{
					Logger.LogError("Unable to find the outgoing Interchange.");
				}
				else
				{
					response = BRMessageHelper.DeserializeObject<LoteValidacaoVersaoDTO>(message.EM_MessageText);
					if (response == null)
					{
						Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
					}
					else
					{
						request = BRMessageHelper.DeserializeObject<FabricanteIntegracaoDTO[]>(outgoingInterchange.EI_BodyText)?.FirstOrDefault(x => x.seq == response.seq);
						if (request == null)
						{
							Logger.LogError($"Unable to find the related object with 'seq' tag equal to {response.seq}.");
						}
					}
				}
			}

			return (request, response);
		}
	}
}
