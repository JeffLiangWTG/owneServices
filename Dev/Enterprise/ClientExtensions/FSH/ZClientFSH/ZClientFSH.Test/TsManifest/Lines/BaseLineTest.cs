using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	public abstract class BaseLineTest : TestCase
	{
		#region VesselVoyage
		protected VesselVoyageLine VesselVoyage
		{
			get
			{
				if (fVesselVoyage == null)
				{
					fVesselVoyage = new VesselVoyageLine(VesselVoyageRow);
				}

				return fVesselVoyage;
			}
		}

		VesselVoyageLine fVesselVoyage;
		protected FortuneShippingDataRow VesselVoyageRow
		{
			get
			{
				return fVesselVoyageRow;
			}

			set
			{
				fVesselVoyageRow = value;
				fVesselVoyage = null;
			}
		}

		FortuneShippingDataRow fVesselVoyageRow;
		#endregion
		#region OceanBill
		protected OceanBillLine OceanBill
		{
			get
			{
				if (fOceanBill == null)
				{
					fOceanBill = VesselVoyage.AddNewOceanBill(OceanBillRow);
				}

				return fOceanBill;
			}
		}

		OceanBillLine fOceanBill;
		protected FortuneShippingDataRow OceanBillRow
		{
			get
			{
				return fOceanBillRow;
			}

			set
			{
				fOceanBillRow = value;
				fOceanBill = null;
			}
		}

		FortuneShippingDataRow fOceanBillRow;
		#endregion
		#region HouseBill
		protected HouseBillLine HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = OceanBill.AddNewHouseBill(HouseBillRow);
				}

				return fHouseBill;
			}
		}

		HouseBillLine fHouseBill;
		protected FortuneShippingDataRow HouseBillRow
		{
			get
			{
				return fHouseBillRow;
			}

			set
			{
				fHouseBillRow = value;
				fHouseBill = null;
			}
		}

		FortuneShippingDataRow fHouseBillRow;
		#endregion
		#region PlaceInfo
		protected PlaceInfoLine PlaceInfo
		{
			get
			{
				if (fPlaceInfo == null)
				{
					fPlaceInfo = OceanBill.AddNewPlaceInfo(PlaceInfoRow);
				}

				return fPlaceInfo;
			}
		}

		PlaceInfoLine fPlaceInfo;
		protected FortuneShippingDataRow PlaceInfoRow
		{
			get
			{
				return fPlaceInfoRow;
			}

			set
			{
				fPlaceInfoRow = value;
				fPlaceInfo = null;
			}
		}

		FortuneShippingDataRow fPlaceInfoRow;
		#endregion
		#region PartyDetails
		protected PartyDetailsLine ShipperDetails
		{
			get
			{
				if (fShipperDetails == null)
				{
					fShipperDetails = OceanBill.AddNewPartyDetails(ShipperDetailsRow);
				}

				return fShipperDetails;
			}
		}

		PartyDetailsLine fShipperDetails;
		protected PartyDetailsLine ConsigneeDetails
		{
			get
			{
				if (fConsigneeDetails == null)
				{
					fConsigneeDetails = OceanBill.AddNewPartyDetails(ConsigneeDetailsRow);
				}

				return fConsigneeDetails;
			}
		}

		PartyDetailsLine fConsigneeDetails;
		protected FortuneShippingDataRow ShipperDetailsRow
		{
			get
			{
				return fShipperDetailsRow;
			}

			set
			{
				fShipperDetailsRow = value;
				fShipperDetails = null;
			}
		}

		FortuneShippingDataRow fShipperDetailsRow;
		protected FortuneShippingDataRow ConsigneeDetailsRow
		{
			get
			{
				return fConsigneeDetailsRow;
			}

			set
			{
				fConsigneeDetailsRow = value;
				fConsigneeDetails = null;
			}
		}

		FortuneShippingDataRow fConsigneeDetailsRow;
		#endregion
		#region CargoField
		protected CargoFieldLine CargoField
		{
			get
			{
				if (fCargoField == null)
				{
					fCargoField = OceanBill.AddNewCargoField(CargoFieldRow);
				}

				return fCargoField;
			}
		}

		CargoFieldLine fCargoField;
		protected FortuneShippingDataRow CargoFieldRow
		{
			get
			{
				return fCargoFieldRow;
			}

			set
			{
				fCargoFieldRow = value;
				fCargoField = null;
			}
		}

		FortuneShippingDataRow fCargoFieldRow;
		#endregion
		#region ContainerField
		protected ContainerFieldLine ContainerField
		{
			get
			{
				if (fContainerField == null)
				{
					fContainerField = OceanBill.AddNewContainerField(ContainerFieldRow);
				}

				return fContainerField;
			}
		}

		ContainerFieldLine fContainerField;
		protected FortuneShippingDataRow ContainerFieldRow
		{
			get
			{
				return fContainerFieldRow;
			}

			set
			{
				fContainerFieldRow = value;
				fContainerField = null;
			}
		}

		FortuneShippingDataRow fContainerFieldRow;
		#endregion
		#region GoodsDescription
		protected CargoDescriptionLine GoodsDescription
		{
			get
			{
				if (fGoodsDescription == null)
				{
					fGoodsDescription = CargoField.AddNewGoodsDescription(GoodsDescriptionRow);
				}

				return fGoodsDescription;
			}
		}

		CargoDescriptionLine fGoodsDescription;
		protected FortuneShippingDataRow GoodsDescriptionRow
		{
			get
			{
				return fGoodsDescriptionRow;
			}

			set
			{
				fGoodsDescriptionRow = value;
				fGoodsDescription = null;
			}
		}

		FortuneShippingDataRow fGoodsDescriptionRow;
		#endregion
		#region MarksAndNumbers
		protected CargoDescriptionLine MarksAndNumbers
		{
			get
			{
				if (fMarksAndNumbers == null)
				{
					fMarksAndNumbers = CargoField.AddNewMarksAndNumbers(MarksAndNumbersRow);
				}

				return fMarksAndNumbers;
			}
		}

		CargoDescriptionLine fMarksAndNumbers;
		protected FortuneShippingDataRow MarksAndNumbersRow
		{
			get
			{
				return fMarksAndNumbersRow;
			}

			set
			{
				fMarksAndNumbersRow = value;
				fMarksAndNumbers = null;
			}
		}

		FortuneShippingDataRow fMarksAndNumbersRow;
		#endregion
		#region Hazardous
		protected HazardousLine Hazardous
		{
			get
			{
				if (fHazardous == null)
				{
					fHazardous = CargoField.AddNewHazardous(HazardousRow);
				}

				return fHazardous;
			}
		}

		HazardousLine fHazardous;
		protected FortuneShippingDataRow HazardousRow
		{
			get
			{
				return fHazardousRow;
			}

			set
			{
				fHazardousRow = value;
				fHazardous = null;
			}
		}

		FortuneShippingDataRow fHazardousRow;
		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			VesselVoyageRow = new FortuneShippingDataRow(new ZString[] { "10", "8707434", "AFRICA STAR", "CY", "504S", "", "", "20051021", "20051022", "AUADL", "ADELAIDE", "", "", "" }, 0);
			OceanBillRow = new FortuneShippingDataRow(new ZString[] { "12", "8PGUADL400700", "", "", "", "MYPGU", "PASIR GUDANG", "MYPKG", "PORT KELANG", "CY-CY", "P", "20051022", "", "", "", "" }, 0);
			PlaceInfoRow = new FortuneShippingDataRow(new ZString[] { "13", "AUADL", "ADELAIDE", "AUADL", "ADELAIDE", "", "", "", "", "11" }, 0);
			ConsigneeDetailsRow = new FortuneShippingDataRow(new ZString[] { "17", "SCHEN AU", "SCHENKER & CO (AUSTR.) PTY. LTD.", "5 FREDERICK ROAD", "ROYAL PARK (ADELAIDE)SA 5014", "P.O.BOX 166", "AUS-PORT ADELAIDE,SA 5014" }, 0);
			ShipperDetailsRow = new FortuneShippingDataRow(new ZString[] { "16", "HASHKHH1", "HANSEN SHIPPING AGENCIES CO., LTD.", "3F, NO. 165-2, FU-HSING 3RD ROAD,", "KAOHSIUNG, TAIWAN R.O.C.", "TEL:07-3324-4478 FAX:07-333-6561", "ATTN:MS.CHEN" }, 0);
			CargoFieldRow = new FortuneShippingDataRow(new ZString[] { "41", "1", "GE", "64", "PK", "PACKAGES", "22832.77", "0", "52.2" }, 0);
			ContainerFieldRow = new FortuneShippingDataRow(new ZString[] { "51", "1", "CCLU4398759", "D851255", "42G1", "F", "64", "22832.7", "3650", "52.2", "", "", "", "", "", "" }, 0);
			GoodsDescriptionRow = new FortuneShippingDataRow(new ZString[] { "47", "1 X 40'GP CONTAINER STC", "-^64 PACKAGES^PALLET RACKING^^MYJHB4030501603^^FREIGHT PREPAID" }, 0);
			MarksAndNumbersRow = new FortuneShippingDataRow(new ZString[] { "44", "N/M" }, 0);
			HazardousRow = new FortuneShippingDataRow(new ZString[] { "43", "5.1", "66", "1479", "0", "", "", "", "", "", "", "", "" }, 0);
			HouseBillRow = new FortuneShippingDataRow(new ZString[] { "61", "HOUSE110", "5", "BL2994" }, 0);
		}
	}
}
