using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers
{
	class ADNCollectionSummaryWriter : IUNDGSubstanceCollectionSummaryWriter
	{
		ZString IUNDGSubstanceCollectionSummaryWriter.GetSummary(IReadOnlyCollection<UNDGSubstanceWrapper> collection)
		{
			var adnUNDGs = collection.Where(IsADN);
			var allNoteSummaries = new List<ZString>();

			foreach (var shipment in GetParentShipments(adnUNDGs))
			{
				if (ShipmentHasBargeLegLoadingAndDischargingInADNCountry(shipment)
					&& GetAdditionalHandlingInformationNoteSummariesFromShipment(shipment) is IEnumerable<ZString> shipmentNoteSummaries)
				{
					allNoteSummaries.AddRange(shipmentNoteSummaries);
				}
			}

			return string.Join(System.Environment.NewLine, allNoteSummaries);
		}

		#region Implementation

		IEnumerable<ZString> GetAdditionalHandlingInformationNoteSummariesFromShipment(ForwardingShipment shipment)
		{
			return shipment?
				.Notes?
				.FindByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description)
				.Select(note => note.ST_NoteText);
		}

		List<ForwardingShipment> GetParentShipments(IEnumerable<UNDGSubstanceWrapper> wrappers)
		{
			var result = new Dictionary<ZGuid, ForwardingShipment>();

			foreach (var wrapper in wrappers)
			{
				if (wrapper?.DGData is ForwardingUNDGDataItem forwardingDataItem
					&& forwardingDataItem.ParentPackLine?.Shipment is ForwardingShipment parentShipment)
				{
					result[parentShipment.PK] = parentShipment;
				}
			}

			return result.Values.ToList();
		}

		bool IsADN(UNDGSubstanceWrapper wrapper)
		{
			return (wrapper.DGData?.Subs?.DG_Standard ?? ZString.Empty) == UNDGSubstanceStandardTypes.ADN;
		}

		bool ShipmentHasBargeLegLoadingAndDischargingInADNCountry(ForwardingShipment parentShipment)
		{
			var shipmentBargeLegs = parentShipment
				.TransportsIncludingRelated.OfType<Transport>()
				.Where(LegIsBargeLoadingAndDischargingInADNCountry)
				.ToArray();

			return shipmentBargeLegs.Length > 0;
		}

		bool LegIsBargeLoadingAndDischargingInADNCountry(Transport transportLeg)
		{
			return TransportLegIsBargeLeg(transportLeg)
				&& LegLoadsandDischargesInADNCountry(transportLeg);
		}

		bool TransportLegIsBargeLeg(Transport transportLeg)
		{
			return (transportLeg.JW_TransportMode == Constants.TransportModes.Sea || transportLeg.JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport) &&
				transportLeg.Vessel != null &&
				transportLeg.Vessel.RV_VesselType == Constants.VesselType.Barge;
		}

		bool LegLoadsandDischargesInADNCountry(Transport transportLeg)
		{
			return ADNContractingCountries.Contains(transportLeg.JW_RL_NKLoadPort.SubstringSafe(0, 2))
				&& ADNContractingCountries.Contains(transportLeg.JW_RL_NKDiscPort.SubstringSafe(0, 2));
		}

		HashSet<ZString> ADNContractingCountries => adnContractingCountries ?? (adnContractingCountries = new HashSet<ZString>()
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

		HashSet<ZString> adnContractingCountries;

		#endregion
	}
}
