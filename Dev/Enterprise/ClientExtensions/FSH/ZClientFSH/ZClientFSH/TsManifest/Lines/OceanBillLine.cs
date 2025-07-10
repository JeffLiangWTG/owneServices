using System.Collections.Generic;
using CargoWise.Types;
using PaymentType = Enterprise.DataTransfer.Xml.XsdVersion1.PaymentType;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class OceanBillLine : BaseLine
	{
		public OceanBillLine(VesselVoyageLine vesselVoyage, FortuneShippingDataRow row) : base(row)
		{
			this.VesselVoyage = vesselVoyage;
		}

		#region Properties

		public ZString OceanBillNumber
		{
			get { return Row[Constants.FirstRecordOf1BL.BLNo]; }
		}

		public PaymentType MethodOfPayment
		{
			get { return Row[Constants.FirstRecordOf1BL.PrepaidOrCollect] == Constants.PaymentMethods.Prepay ? PaymentType.PPD : PaymentType.CCX; }
		}

		public ZString PortOfOrigin
		{
			get { return Row[Constants.FirstRecordOf1BL.PlaceCodeOfReceipt]; }
		}

		public ZString PortOfLoading
		{
			get { return Row[Constants.FirstRecordOf1BL.LoadPortCode]; }
		}

		#endregion

		#region RelatedLines

		#region PlaceInfo

		public PlaceInfoLine AddNewPlaceInfo(FortuneShippingDataRow row)
		{
			PlaceInfoLine result = new PlaceInfoLine(this, row);
			fPlaceInfo = result;
			return result;
		}

		public PlaceInfoLine PlaceInfo
		{
			get { return fPlaceInfo; }
		}
		protected PlaceInfoLine fPlaceInfo;

		#endregion

		#region PartyDetails

		public PartyDetailsLine AddNewPartyDetails(FortuneShippingDataRow row)
		{
			PartyDetailsLine result = new PartyDetailsLine(this, row);

			switch (row.RowType)
			{
				case Constants.RecordIDs.ShipperFields:
					fShipperDetails = result;
					break;

				case Constants.RecordIDs.ConsigneeFields:
					fConsigneeDetails = result;
					break;
			}

			return result;
		}

		public PartyDetailsLine ShipperDetails
		{
			get { return fShipperDetails; }
		}
		protected PartyDetailsLine fShipperDetails;

		public PartyDetailsLine ConsigneeDetails
		{
			get { return fConsigneeDetails; }
		}
		protected PartyDetailsLine fConsigneeDetails;

		#endregion

		#region CargoFields

		public CargoFieldLine AddNewCargoField(FortuneShippingDataRow row)
		{
			CargoFieldLine result = new CargoFieldLine(this, row);
			cargoFields.Add(result);
			return result;
		}

		public IReadOnlyList<CargoFieldLine> CargoFields => cargoFields;

		protected List<CargoFieldLine> cargoFields = new List<CargoFieldLine>();

		#endregion

		#region ContainerFields

		public ContainerFieldLine AddNewContainerField(FortuneShippingDataRow row)
		{
			ContainerFieldLine result = new ContainerFieldLine(this, row);
			containerFields.Add(result);

			foreach (CargoFieldLine cargoField in CargoFields)
			{
				if (cargoField.CargoSequenceNumber == result.CargoSequenceNumber)
				{
					cargoField.AttachContainerField(result);
					result.AttachCargoField(cargoField);
					break;
				}
			}

			return result;
		}

		public IReadOnlyList<ContainerFieldLine> ContainerFields => containerFields;

		protected List<ContainerFieldLine> containerFields = new List<ContainerFieldLine>();

		#endregion

		#region HouseBills

		public HouseBillLine AddNewHouseBill(FortuneShippingDataRow row)
		{
			HouseBillLine result = new HouseBillLine(this, row);
			houseBills.Add(result);
			return result;
		}

		public IReadOnlyList<HouseBillLine> HouseBills => houseBills;

		protected List<HouseBillLine> houseBills = new List<HouseBillLine>();

		#endregion

		#region VesselVoyage

		public readonly VesselVoyageLine VesselVoyage;

		#endregion

		#endregion
	}
}
