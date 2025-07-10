using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class GbCDSH7ImportGoodsShipmentWrapper : IGoodsShipment, IConsignment
	{
		public GbCDSH7ImportGoodsShipmentWrapper(AsycudaBill bill, bool isBIRDSMessage)
		{
			this.bill = bill;
			this.isBIRDSMessage = isBIRDSMessage;
		}

		readonly AsycudaBill bill;
		readonly bool isBIRDSMessage;

		IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => bill.PackedItems.Cast<AsycudaPackedItem>().Select(GetLineWrapper);

		GbCDSH7ImportGoodsItemWrapper GetLineWrapper(AsycudaPackedItem packedItem) => new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage);

		public ZString UCRTraderAssignedReferenceID => bill.ABL_UCRNumber;

		public IWareHouse Warehouse => null;

		public IOrganisation Importer => bill.GetImporterOrg();

		public IOrganisation Seller => bill.GetShipperOrg();

		public IOrganisation Buyer => bill.GetConsigneeOrg();

		public IEnumerable<IParty> AEOMutualRecognitionParties => null;

		public IEnumerable<IParty> DomesticDutyTaxParties => null;

		public ITradeTerms TradeTerms => null;

		public ICustomsValuation CustomsValuation => null;

		public ZString ExportCountryID => isBIRDSMessage ? bill.ABL_RL_NKOrigin.SubstringSafe(0, 2) : null;

		public ZString DestinationCountryCode => null;

		public ZString TransactionNatureCode => null;

		public IConsignment Consignment => this;

		public IEnumerable<IPreviousDocument> PreviousDocuments => null;

		public ZString ContainerCode => null;

		public ITransportMeans ArrivalTransportMeans => CachedValueHelper.GetValue(ref arrivalTransportMeans, GetArrivalTransportMeans);
		CachedValue<ITransportMeans> arrivalTransportMeans;

		ITransportMeans GetArrivalTransportMeans()
		{
			var result = default(ITransportMeans);

			if (isBIRDSMessage)
			{
				var identificationTypeCode = GetIdentificationTypeCode(bill.Header.AMA_TransportMode);
				if (!identificationTypeCode.IsEmpty)
				{
					var transportId = GetTransportID(bill.Header.AMA_TransportMode);
					result = TransportMeansWrapper.New(transportId, string.Empty, identificationTypeCode, string.Empty);
				}
			}

			return result;
		}

		ZString GetIdentificationTypeCode(string transportMode)
		{
			return transportMode switch
			{
				Constants.TransportModes.Sea => "11",
				Constants.TransportModes.Rail => "20",
				Constants.TransportModes.Road => "30",
				Constants.TransportModes.Air => "40",
				_ => ZString.Empty,
			};
		}

		ZString GetTransportID(string transportMode)
		{
			var header = bill.Header;

			return transportMode switch
			{
				Constants.TransportModes.Sea => header.AMA_VesselName,
				Constants.TransportModes.Rail => header.AMA_Voyage,
				Constants.TransportModes.Road => string.Join(" ", header.AMA_VehicleRegistration, header.AMA_Trailer1RegNo, header.AMA_Trailer2RegNo),
				Constants.TransportModes.Air => header.AMA_Voyage,
				_ => ZString.Empty,
			};
		}

		public ITransportMeans DepartureTransportMeans => null;

		public IGoodsLocation GoodsLocation => GoodsLocationWrapper.New(
			name: bill.CusGoodsLocation.CGL_AdditionalIdentifier,
			typeCode: bill.CusGoodsLocation.CGL_Type,
			addressTypeCode: bill.CusGoodsLocation.CGL_Qualifier,
			countryCode: Core.Constants.CountryCodes.UnitedKingdom);

		public ZString LoadingLocationID => null;

		public IEnumerable<ITransportEquipment> TransportEquipments => null;

		public IOrganisation Carrier => null;

		public ZString FreightPaymentMethodCode => null;

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => Enumerable.Empty<ZString>();

		public IOrganisation Consignor => bill.GetShipperOrg();

		public IConsignmentItem ConsignmentItem => null;
	}
}
