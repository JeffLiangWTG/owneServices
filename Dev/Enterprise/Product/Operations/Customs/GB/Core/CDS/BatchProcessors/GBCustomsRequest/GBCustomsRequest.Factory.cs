using System;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.CDS
{
	public static class GBCustomsRequestFactory
	{
		public static GBCustomsRequestDataProviderBase GetRequestDataProvider(CDSEDIMessage message)
		{
			switch (message.EM_LinkedObject)
			{
				case CusEntryHeader header:
					{
						var dec = header.Declaration;
						if (dec != null)
						{
							return new GBDeclarationCustomsRequestDataProvider(dec);
						}
					}
					break;
				case ForwardingConsol consol:
					return new GBConsolCustomsRequestDataProvider(consol);
				case AsycudaBill bill:
					return new GBAsycudaBillCustomsRequestDataProvider(bill);
			}

			return null;
		}

		public static GBCustomsRequest New(CDSEDIMessage message)
		{
			GBCustomsRequest request = null;

			var dataProvider = GetRequestDataProvider(message);
			if (dataProvider != null)
			{
				var provider = GetProviderType(dataProvider.Gateway);
				request = new GBCustomsRequest
				{
					JobNumber = dataProvider.JobNumber,
					Provider = provider,
					Credentials = dataProvider.Credentials,
					Service = GetServiceType(message, provider)
				};
			}

			return request;
		}

		internal static ServiceType GetServiceType(CDSEDIMessage message, ProviderType provider)
		{
			switch (message)
			{
				case CDSArrivalAmendmentDeclarationEDIMessage _:
					if (provider == ProviderType.Direct)
					{
						return ServiceType.GoodsPresentationNotification;
					}
					else if (provider == ProviderType.Pentant)
					{
						return ServiceType.PentantArrival;
					}
					else
					{
						return ServiceType.NewDeclaration;
					}
				case CDSNewDeclarationEDIMessage _:
					return ServiceType.NewDeclaration;
				case CDSAmendDeclarationEDIMessage _:
					return ServiceType.AmendDeclaration;
				case CDSCancelDeclarationEDIMessage _:
					return ServiceType.CancelDeclaration;
				case CDSInventoryLinkingQueryRequestEDIMessage _:
				case CDSInventoryLinkingMasterQueryRequestEDIMessage _:
					if (IsSendingViaCspAndViaEhub(provider))
					{
						return ServiceType.ExportInventoryQuery;
					}
					else
					{
						return ServiceType.ExportInventory;
					}
				case CDSInventoryLinkingConsolidationRequestEDIMessage _:
					if (IsSendingViaCspAndViaEhub(provider))
					{
						return ServiceType.ExportInventoryConsolidation;
					}
					else
					{
						return ServiceType.ExportInventory;
					}
				case CDSInventoryLinkingMovementRequestEDIMessage _:
					if (IsSendingViaCspAndViaEhub(provider))
					{
						return ServiceType.ExportInventoryMovement;
					}
					else
					{
						return ServiceType.ExportInventory;
					}
				case CDSPentantAcaMessage _:
					return ServiceType.PentantACA;
				default:
					throw new NotSupportedException(FormattableString.Invariant($"{message.EM_MessageType} is not supported."));
			}
		}

		public static bool IsSendingViaCspAndViaEhub(ProviderType provider) => provider == ProviderType.CNS || provider == ProviderType.MCP || provider == ProviderType.Pentant;

		public static ProviderType GetProviderType(ZString gateWay)
		{
			switch (gateWay)
			{
				case GatewayList.Codes.CCSUKviaNTMsgGW:
					return ProviderType.CCSUK;
				case GatewayList.Codes.CNS_CUSDECOnly:
					return ProviderType.CNS;
				case GatewayList.Codes.MCP_CUSDECOnly:
					return ProviderType.MCP;
				case GatewayList.Codes.Pentant:
					return ProviderType.Pentant;
				default:
					return ProviderType.Direct;
			}
		}
	}
}
