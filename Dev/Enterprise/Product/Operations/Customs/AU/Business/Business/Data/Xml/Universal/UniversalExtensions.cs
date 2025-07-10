using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class UniversalExtensions
	{
		internal const string TILV4Warehouse = "TILV4Warehouse";

		public static UniversalAddInfo GetTILV4Warehouse(this JobComInvoiceLine invoiceLine)
		{
			var transportAndInsuranceInLocalCurrency = invoiceLine.TransportAndInsuranceInLocalCurrency;
			UniversalAddInfo result = null;
			if (!transportAndInsuranceInLocalCurrency.IsEmpty)
			{
				result = new UniversalAddInfo();
				result.Key = TILV4Warehouse;
				result.Value = invoiceLine.TransportAndInsuranceInLocalCurrency.ToString();
			}
			return result;
		}

		public static UniversalAddInfo GetTILV4Warehouse(this List<UniversalAddInfo> addInfos)
		{
			UniversalAddInfo result = null;
			if (addInfos != null)
			{
				result = addInfos.FirstOrDefault(x => x.Key.HasValue && x.Key.Value == TILV4Warehouse);
			}
			return result;
		}

		public static UniversalAddInfo GetEntryNumberForWarehouse(this List<UniversalAddInfo> addInfos)
		{
			UniversalAddInfo result = null;
			if (addInfos != null)
			{
				result = addInfos.FirstOrDefault(x => x.Key.HasValue && x.Key.Value == "WRN");
			}
			return result;
		}

		public static UniversalAddInfo GetEntryLineNumberForWarehouse(this List<UniversalAddInfo> addInfos)
		{
			UniversalAddInfo result = null;
			if (addInfos != null)
			{
				result = addInfos.FirstOrDefault(x => x.Key.HasValue && x.Key.Value == "WRL");
			}
			return result;
		}

		public static TransportLeg GetArrivalTransportLeg(this Shipment shipment, TransportMode transportMode)
		{
			TransportLeg result = null;
			if (shipment.TransportLegCollection != null)
			{
				result = shipment.TransportLegCollection.OrderBy(x => x.LegOrder).FirstOrDefault(x => x.TransportMode.GetValueOrDefault() == transportMode
					&& !IsAU(x.PortOfLoading) && IsAU(x.PortOfDischarge));
			}
			return result;
		}

		static bool IsAU(UNLOCO unloco)
		{
			return unloco?.Code?.StartsWith(Core.Constants.CountryCodes.Australia, StringComparison.OrdinalIgnoreCase) ?? false;
		}

		#region NEXDOCS

		public static bool IsNEXDOCS(this IXmlEventValueObject eventData)
		{
			var provider = eventData.DataContext?.DataProviderForCodeMapping ?? ZString.Empty;
			return provider == Constants.DataProvider.NEXDOCS || provider == Constants.DataProvider.NEXDOCSTest;
		}

		public static bool IsNEXDOCSCertificatePrint(this IXmlEventValueObject eventData)
		{
			return eventData.EventType == Events.DocumentImportedCode
				&& (string)eventData.DataContext?.ActionPurposeCode == Constants.ActionPurpose.ADD;
		}

		public static bool IsNEXDOCSMessageReceived(this IXmlEventValueObject eventData)
		{
			return eventData.EventType == Events.MessageReceivedCode;
		}

		public static bool IsNEXDOCSNotification(this IXmlEventValueObject eventData)
		{
			return eventData.EventReference == Constants.EventReference.Notify;
		}

		#endregion
	}
}
