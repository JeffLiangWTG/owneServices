using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.FSH.TsManifest.Lines;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.FSH.TsManifest
{
	public class FortuneShippingFlatFileConverter : FlatFileConverter
	{
		public FortuneShippingFlatFileConverter(INotifications subscriber, BusinessObjectFactory factory) : base(subscriber, factory)
		{
		}

		#region MapImport

		protected override void MapImport(DataTransfer.Xml.IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.ConsolCollection consols = (Xsd.ConsolCollection)valueObject;

			VesselVoyageLine vesselVoyage = null;
			OceanBillLine currentOceanBill = null;
			CargoFieldLine currentCargoField = null;

			foreach (FlatFileDataRow line in fileLines)
			{
				FortuneShippingDataRow row = (FortuneShippingDataRow)line;
				switch (row.RowType)
				{
					case Constants.RecordIDs.VslAndVoyFields:
						vesselVoyage = new VesselVoyageLine(row);
						break;

					case Constants.RecordIDs.FirstRecordOf1BL:
						currentOceanBill = vesselVoyage.AddNewOceanBill(row);
						break;

					case Constants.RecordIDs.PlaceInfoOf1BL:
						currentOceanBill.AddNewPlaceInfo(row);
						break;

					case Constants.RecordIDs.ShipperFields:
					case Constants.RecordIDs.ConsigneeFields:
						currentOceanBill.AddNewPartyDetails(row);
						break;

					case Constants.RecordIDs.CargoFields:
						currentCargoField = currentOceanBill.AddNewCargoField(row);
						break;

					case Constants.RecordIDs.Hazardous:
						currentCargoField.AddNewHazardous(row);
						break;

					case Constants.RecordIDs.CargoDescription:
						currentCargoField.AddNewGoodsDescription(row);
						break;

					case Constants.RecordIDs.CargoMarks:
						currentCargoField.AddNewMarksAndNumbers(row);
						break;

					case Constants.RecordIDs.ContainerFieldsRecord:
						currentOceanBill.AddNewContainerField(row);
						break;

					case Constants.RecordIDs.HouseColoBillInfo:
						currentOceanBill.AddNewHouseBill(row);
						break;

					default:
						break;
				}
			}

			MapImportToConsol(vesselVoyage, consols);
		}

		void MapImportToConsol(VesselVoyageLine vesselVoyage, Xsd.ConsolCollection consols)
		{
			if (vesselVoyage != null)
			{
				foreach (OceanBillLine oceanBill in vesselVoyage.OceanBills)
				{
					Xsd.Consol consol = consols.AddNew();

					Xsd.ConsolIdentifier identifier = new Xsd.ConsolIdentifier();
					identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
					identifier.Value = oceanBill.OceanBillNumber;
					consol.ConsolIdentifier.Add(identifier);

					consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
					consol.ConsolDetail.TransportModeSpecified = true;
					if (oceanBill.PlaceInfo != null)
					{
						consol.ConsolDetail.PortOfDischarge.Port.Value = oceanBill.PlaceInfo.PortOfDischarge;
					}

					consol.ConsolDetail.PortOfLoading.Port.Value = oceanBill.PortOfLoading;

					Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();
					sailing.VesselName = vesselVoyage.Vessel;
					sailing.LloydsNo = vesselVoyage.VesselCode;
					sailing.VoyageNo = vesselVoyage.Voyage;
					consol.ConsolDetail.Item = sailing;

					consol.ConsolDetail.PaymentType = oceanBill.MethodOfPayment;
					consol.ConsolDetail.PaymentTypeSpecified = true;

					consol.ConsolDetail.ConsolType = Xsd.ConsolType.Agent;
					consol.ConsolDetail.ConsolTypeSpecified = true;
					SetOrgDetails(consol.ConsolDetail.SendingAgent, SendingAgentCode);
					SetOrgDetails(consol.ConsolDetail.ReceivingAgent, ReceivingAgentCode);
					SetOrgDetails(consol.ConsolDetail.Carrier, CarrierCode);

					foreach (ContainerFieldLine containerField in oceanBill.ContainerFields)
					{
						Xsd.Container container = consol.ConsolDetail.Containers.AddNew();
						container.ContainerNumber = containerField.ContainerNumber;
						container.ContainerType.ISOCode = containerField.ContainerSizeOrISOCode;

						container.IsShipperOwnedContainer = new ZBool(containerField.ShipperOwned);
						container.PackingMode = containerField.ContainerMode;
						consol.ConsolDetail.ContainerMode = containerField.ContainerMode;
						consol.ConsolDetail.ContainerModeSpecified = true;
						container.Seal = containerField.SealNumber;
					}

					foreach (CargoFieldLine cargoField in oceanBill.CargoFields)
					{
						Xsd.Shipment shipment = consol.Shipments.AddNew();

						Xsd.ShipmentIdentifier shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
						shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
						// TODO: Use a housebill if one is present. (though they don't seem to use them)
						shipmentIdentifier.Value = oceanBill.OceanBillNumber;
						shipmentIdentifier.Masterbill = oceanBill.OceanBillNumber;

						FromPartyDetailsLine(oceanBill.ConsigneeDetails, shipment.ShipmentDetails.Consignee);
						FromPartyDetailsLine(oceanBill.ShipperDetails, shipment.ShipmentDetails.Consignor);

						shipment.ShipmentDetailsSpecified = true;

						shipment.ShipmentDetails.GoodsDescription = cargoField.CompleteGoodsDescription;
						Xsd.NotesNote goodsNote = shipment.Notes.AddNew();
						goodsNote.NoteType = Xsd.NotesNoteNoteType.DetailedGoodsDescription;
						goodsNote.NoteData = cargoField.CompleteGoodsDescription;

						shipment.ShipmentDetails.MarksAndNumbers = cargoField.CompleteMarksAndNumbers;
						Xsd.NotesNote marksNote = shipment.Notes.AddNew();
						marksNote.NoteType = Xsd.NotesNoteNoteType.MarksAndNumbers;
						marksNote.NoteData = cargoField.CompleteMarksAndNumbers;

						if (oceanBill.PlaceInfo != null)
						{
							shipment.ShipmentDetails.PortofDestination.Port.Value = oceanBill.PlaceInfo.PortOfDestination;
						}

						shipment.ShipmentDetails.PortOfOrigin.Port.Value = oceanBill.PortOfOrigin;

						shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

						shipment.ShipmentDetails.Volume.DimensionType = VolumeUnit;
						shipment.ShipmentDetails.Weight.DimensionType = WeightUnit;
						shipment.ShipmentDetails.Volume.Value = cargoField.GrossCube;
						shipment.ShipmentDetails.Weight.Value = cargoField.GrossWeight;

						shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = cargoField.CargoType;
						shipment.ShipmentDetails.TotalOuterPacksQty.Value = cargoField.NumberOfPackages;

						foreach (ContainerFieldLine containerField in cargoField.ContainerFields)
						{
							Xsd.Package package = shipment.ShipmentDetails.Packages.AddNew();

							package.ContainerNumber = containerField.ContainerNumber;
							package.GoodsDescription = cargoField.CompleteGoodsDescription;

							if (cargoField.Hazardous != null)
							{
								Xsd.HazardousGoods xsdDG = package.DangerousGoods.AddNew();
								xsdDG.FlashPoint = cargoField.Hazardous.FlashPoint;
								xsdDG.IMOClass = cargoField.Hazardous.IMOClass;
								xsdDG.UNDGCode = cargoField.Hazardous.IMOUNNumber;
							}

							package.NumberOfPacks = uint.Parse(containerField.NumberOfPackages);
							package.Origin = oceanBill.PortOfOrigin.SubstringSafe(0, 2);
							package.PackType = cargoField.CargoType;

							package.Volume.Value = containerField.Volume;
							package.Volume.DimensionType = VolumeUnit;
							package.Weight.Value = containerField.NetWeight;
							package.Weight.DimensionType = WeightUnit;

							shipment.ShipmentDetails.PackingMode = containerField.ContainerMode;
						}
					}
				}
			}
		}

		const string SendingAgentCode = "RICSHI";
		const string ReceivingAgentCode = "FORSHI";
		const string CarrierCode = "CHISHI";
		const string VolumeUnit = "M3";
		const string WeightUnit = "KG";

		void FromPartyDetailsLine(PartyDetailsLine partyDetails, Xsd.Organisation organisation)
		{
			if (partyDetails != null)
			{
				organisation.EDICode = partyDetails.AddressCode;
				organisation.OrganisationDetails.Name = partyDetails.Address1;

				Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.AddNew();
				mainAddress.AddressLine1 = partyDetails.Address2;
				mainAddress.AddressLine2 = partyDetails.Address3;
				mainAddress.StateOrProvince = partyDetails.Address4;
				mainAddress.CityOrSuburb = partyDetails.Address5;
			}
		}

		void SetOrgDetails(Xsd.Organisation org, string code)
		{
			org.EDICode = code;
			org.OrganisationDetails.Name = code;
			org.OrganisationDetails.Addresses.AddNew().AddressLine1 = code;
		}

		#endregion

		#region ImportFlatFile

		public override void ImportFlatFile(DataTransfer.Xml.IValueObject valueObject, IFlatFileFormat fileFormat, TextReader flatFileReader)
		{
			string preprocessedData = PreprocessData(flatFileReader);
			base.ImportFlatFile(valueObject, fileFormat, new StringReader(preprocessedData));
		}

		#endregion

		protected override FlatFileDataRow ExtractRowFromFormat(IFlatFileFormat fileFormat, ZString rawRow)
		{
			ZString trimmedRow = rawRow.TrimEnd(new char[] { '\'' });
			return base.ExtractRowFromFormat(fileFormat, trimmedRow);
		}

		#region Implementation

		#region PreprocessData

		internal string PreprocessData(TextReader flatFileReader)
		{
			StringBuilder result = new StringBuilder();

			string line;
			while ((line = flatFileReader.ReadLine()) != null)
			{
				string trimmedLine = line.TrimStart(new char[] { '^' });

				if (trimmedLine.Length != line.Length)
				{
					result.Append(" ");
				}

				result.Append(trimmedLine);

				if (line.EndsWith("'"))
				{
					result.Append(System.Environment.NewLine);
				}
			}

			return result.ToString();
		}

#endregion
#endregion
					}
}
