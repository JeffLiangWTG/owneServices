using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	class ADRTunnelRestrictionComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => false;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var dataItem = wrapper?.DGData as ForwardingUNDGDataItem;
			var parentShipment = dataItem?.ParentPackLine?.Shipment;

			if (parentShipment != null)
			{
				if (ShipmentHasRoadLegLoadingAndDischargingInADRCountry(parentShipment))
				{
					var tunnelCode = (dataItem?.Subs?.StandardSubstance as UNDGSubstanceADR)?.ADR_TransportCategory ?? ZString.Empty;
					var regex = new Regex(@"^.*\((.*)\)$");
					var matchedTunnelCode = new ZString(regex.Match(tunnelCode).Groups[1].Value);

					if (!OrderedTunnelRestrictions.Contains(matchedTunnelCode))
					{
						return ZString.Empty;
					}

					return ADRTunnelRestrictionCodeIsMostRestrictive(parentShipment, matchedTunnelCode)
						? new ZString($"({matchedTunnelCode} [MOST RESTRICTIVE])")
						: ZString.Empty;
				}
			}

			return ZString.Empty;
		}

		bool ShipmentHasRoadLegLoadingAndDischargingInADRCountry(ForwardingShipment shipment)
		{
			var shipmentRoadLegs = shipment
				.TransportsIncludingRelated.OfType<Transport>()
				.Where(transport => transport.JW_TransportMode == Constants.TransportModes.Road);

			return shipmentRoadLegs.Any(LegLoadsandDischargesInADRCountry);
		}

		bool LegLoadsandDischargesInADRCountry(Transport roadLeg)
		{
			return ADRTunnelCodeCountries.Contains(roadLeg.JW_RL_NKLoadPort.SubstringSafe(0, 2))
				&& ADRTunnelCodeCountries.Contains(roadLeg.JW_RL_NKDiscPort.SubstringSafe(0, 2));
		}

		bool ADRTunnelRestrictionCodeIsMostRestrictive(ForwardingShipment shipment, ZString tunnelCode)
		{
			var allDangerousGoods = shipment
				.OuterPackLines
				.OfType<ForwardingPackLine>()
				.SelectMany(pl => pl.UNDGs);

			var regex = new Regex(@"^.*\((.*)\)$");

			var shipmentTunnelCodes = allDangerousGoods
				.Select(dg => dg.Subs)
				.Where(substance => substance != null && substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR)
				.Select(substance => ((UNDGSubstanceADR)substance.StandardSubstance).ADR_TransportCategory)
				.Where(code => !code.IsEmpty);

			var allRestrictionsOfShipment = shipmentTunnelCodes
				.Select(code => regex.Match(code).Groups[1].Value)
				.Distinct()
				.Where(code => OrderedTunnelRestrictions.Contains(code))
				.OrderBy(code => Array.IndexOf(OrderedTunnelRestrictions, code));

			return string.Compare(tunnelCode, allRestrictionsOfShipment.FirstOrDefault(), StringComparison.OrdinalIgnoreCase) == 0;
		}

		HashSet<ZString> ADRTunnelCodeCountries => adrTunnelCodeCountries ?? (adrTunnelCodeCountries = new HashSet<ZString>()
		{
			Constants.CountryCodes.Albania,
			Constants.CountryCodes.Andorra,
			Constants.CountryCodes.Armenia,
			Constants.CountryCodes.Austria,
			Constants.CountryCodes.Azerbaijan,
			Constants.CountryCodes.Belarus,
			Constants.CountryCodes.Belgium,
			Constants.CountryCodes.BosniaAndHerzegovina,
			Constants.CountryCodes.Bulgaria,
			Constants.CountryCodes.Croatia,
			Constants.CountryCodes.Cyprus,
			Constants.CountryCodes.CzechRepublic,
			Constants.CountryCodes.Denmark,
			Constants.CountryCodes.Estonia,
			Constants.CountryCodes.Finland,
			Constants.CountryCodes.France,
			Constants.CountryCodes.Georgia,
			Constants.CountryCodes.Germany,
			Constants.CountryCodes.Greece,
			Constants.CountryCodes.Hungary,
			Constants.CountryCodes.Iceland,
			Constants.CountryCodes.Ireland,
			Constants.CountryCodes.Italy,
			Constants.CountryCodes.Kazakhstan,
			Constants.CountryCodes.Latvia,
			Constants.CountryCodes.Liechtenstein,
			Constants.CountryCodes.Lithuania,
			Constants.CountryCodes.Luxembourg,
			Constants.CountryCodes.Malta,
			Constants.CountryCodes.Montenegro,
			Constants.CountryCodes.Morocco,
			Constants.CountryCodes.Netherlands,
			Constants.CountryCodes.Nigeria,
			Constants.CountryCodes.Norway,
			Constants.CountryCodes.Poland,
			Constants.CountryCodes.Portugal,
			Constants.CountryCodes.Moldova,
			Constants.CountryCodes.Romania,
			Constants.CountryCodes.Russia,
			Constants.CountryCodes.SanMarino,
			Constants.CountryCodes.Serbia,
			Constants.CountryCodes.Slovakia,
			Constants.CountryCodes.Slovenia,
			Constants.CountryCodes.Spain,
			Constants.CountryCodes.Sweden,
			Constants.CountryCodes.Switzerland,
			Constants.CountryCodes.Tajikistan,
			Constants.CountryCodes.Macedonia,
			Constants.CountryCodes.Tunisia,
			Constants.CountryCodes.Turkey,
			Constants.CountryCodes.Ukraine,
			Constants.CountryCodes.UnitedKingdom,
			Constants.CountryCodes.Uzbekistan
		});

		HashSet<ZString> adrTunnelCodeCountries;

		#region SuppressResourceStringsCheckRegion

		ZString[] OrderedTunnelRestrictions => orderedTunnelRestrictions ?? (orderedTunnelRestrictions = new ZString[]
		{
			"B",
			"B1000C",
			"B/D",
			"B/E",
			"C",
			"C5000D",
			"C/D",
			"C/E",
			"D",
			"D/E",
			"E"
		});

		ZString[] orderedTunnelRestrictions;

		#endregion
	}
}
