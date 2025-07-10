using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers
{
	class ADNStandardSummaryWriter : IUNDGStandardSummaryWriter
	{
		IReadOnlyCollection<IUNDGSummaryWriterComponent> IUNDGStandardSummaryWriter.Components
		{
			get
			{
				return new IUNDGSummaryWriterComponent[]
				{
					new UNNOComponent(),
					new PSNComponent(),
					new ClassComponent(),
					new PackingGroupComponent(),
					new FlashPointComponent(),
					new LimitedQuantityComponent(false),
					new PSAGroupComponent(),
					new ExceptedQuantityComponent(),
					new MarinePollutantComponent(),
				}.Concat(GetRadioactiveComponents()).ToArray();
			}
		}

		bool IUNDGStandardSummaryWriter.StandardConditionApplies(UNDGSubstanceWrapper wrapper)
		{
			if (wrapper?.DGData is ForwardingUNDGDataItem dataItem
				&& dataItem.ParentPackLine?.Shipment is ForwardingShipment shipment)
			{
				var allTransportLegs = shipment
					.TransportsIncludingRelated
					.OfType<Transport>();

				return allTransportLegs.Any(TransportLegIsBargeLegInADNContractingCountry);
			}

			return false;
		}

		public static bool TransportLegIsBargeLegInADNContractingCountry(Transport transportLeg)
		{
			var legIsBarge = (transportLeg.JW_TransportMode == Constants.TransportModes.Sea || transportLeg.JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport)
				&& (transportLeg.Vessel?.RV_VesselType ?? ZString.Empty) == Constants.VesselType.Barge;

			if (legIsBarge)
			{
				return ADNContractingCountries.Contains(transportLeg.JW_RL_NKLoadPort.SubstringSafe(0, 2))
					&& ADNContractingCountries.Contains(transportLeg.JW_RL_NKDiscPort.SubstringSafe(0, 2));
			}

			return false;
		}

		#region ADN Contracting Countries

		static HashSet<ZString> ADNContractingCountries => adnContractingCountries ?? (adnContractingCountries = new HashSet<ZString>()
		{
			Constants.CountryCodes.Austria,
			Constants.CountryCodes.Belgium,
			Constants.CountryCodes.Bulgaria,
			Constants.CountryCodes.Croatia,
			Constants.CountryCodes.CzechRepublic,
			Constants.CountryCodes.France,
			Constants.CountryCodes.Germany,
			Constants.CountryCodes.Hungary,
			Constants.CountryCodes.Luxembourg,
			Constants.CountryCodes.Netherlands,
			Constants.CountryCodes.Poland,
			Constants.CountryCodes.Moldova,
			Constants.CountryCodes.Romania,
			Constants.CountryCodes.Russia,
			Constants.CountryCodes.Serbia,
			Constants.CountryCodes.Slovakia,
			Constants.CountryCodes.Switzerland,
			Constants.CountryCodes.Ukraine
		});

		[ThreadStatic]
		static HashSet<ZString> adnContractingCountries;

		#endregion

	}
}
