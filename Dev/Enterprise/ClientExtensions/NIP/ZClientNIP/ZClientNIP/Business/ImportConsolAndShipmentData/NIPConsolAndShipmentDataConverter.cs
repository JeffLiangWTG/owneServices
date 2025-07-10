using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NIP.Business.ConsolAndShipmentImport
{
	public class NIPConsolAndShipmentDataConverter : FlatFileConverter<Xsd.Consol>
	{
		public NIPConsolAndShipmentDataConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		protected override void MapImport(Xsd.Consol valueObject, FlatFileDataRowCollection fileLines)
		{
			if (fileLines.Count > 0)
			{
				ProcessConsolData(valueObject, new NIPConsolAndShipmentFlatFileDataRow(fileLines[0]));

				foreach (FlatFileDataRow dataRow in fileLines)
				{
					NIPConsolAndShipmentFlatFileDataRow nIPDataRow = new NIPConsolAndShipmentFlatFileDataRow(dataRow);
					switch (nIPDataRow.DataKind)
					{
						case NIPConstants.ImportConsolAndShipmentData.ShipmentDataRowCode:
							CurrentShipment = valueObject.Shipments.AddNew();
							ProcessShipment(nIPDataRow);
							break;
						case NIPConstants.ImportConsolAndShipmentData.ContainerDataRowCode:
							ProcessContainer(valueObject, nIPDataRow);
							break;
					}
				}
			}
		}

		void ProcessConsolData(Xsd.Consol consolValue, NIPConsolAndShipmentFlatFileDataRow dataRow)
		{
			#region defaults

			consolValue.ConsolDetail.ConsolType = Xsd.ConsolType.Agent;
			consolValue.ConsolDetail.ConsolTypeSpecified = true;

			#endregion

			consolValue.ConsolDetail.TransportMode = ConsolTransportModeToXmlCodeMappings.Instance.GetExternalCode(dataRow.TransportMode, "", new ValueObjectImportContext(Factory, Notification));
			consolValue.ConsolDetail.TransportModeSpecified = true;

			Xsd.ConsolIdentifier identifier = consolValue.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = dataRow.MasterBLNo;

			consolValue.ConsolDetail.AgentReference = dataRow.ManifestNo;
			consolValue.ConsolDetail.ContainerMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(dataRow.ContainerMode, "", Context);
			consolValue.ConsolDetail.ContainerModeSpecified = true;

			consolValue.ConsolDetail.Carrier = new Xsd.Organisation();
			consolValue.ConsolDetail.Carrier.OwnerCode = dataRow.CarrierCode;

			if (dataRow.TransportMode == Core.Constants.TransportModes.Sea)
			{
				Xsd.SailingWithVesselVoyage vesselDetails = new Xsd.SailingWithVesselVoyage();
				consolValue.ConsolDetail.Item = vesselDetails;

				vesselDetails.VesselName = dataRow.VesselName;
				vesselDetails.LloydsNo = dataRow.LloydsNo;
				vesselDetails.VoyageNo = dataRow.FinalFlightVoyage;

				consolValue.ConsolDetail.PortOfLoading.Port.Value = dataRow.PortOfLoadingCountry + dataRow.PortOfLoadingCity;
				consolValue.ConsolDetail.PortOfDischarge.Port.Value = dataRow.PortOfDischargeCountry + dataRow.PortOfDischargeCity;
				if (dataRow.EstimatedDateOfDeparture.IsValid && !dataRow.EstimatedDateOfDeparture.IsEmpty)
				{
					consolValue.ConsolDetail.PortOfLoading.EstimatedDateTime = dataRow.EstimatedDateOfDeparture.AddHours(Convert.ToInt16(dataRow.EstimatedTimeOfDeparture.Left(2))).AddMinutes(Convert.ToInt16(dataRow.EstimatedTimeOfDeparture.Right(2)));
				}
				if (dataRow.EstimatedDateOfArrival.IsValid && !dataRow.EstimatedDateOfArrival.IsEmpty)
				{
					consolValue.ConsolDetail.PortOfDischarge.EstimatedDateTime = dataRow.EstimatedDateOfArrival.AddHours(Convert.ToInt16(dataRow.EstimatedTimeOfArrival.Left(2))).AddMinutes(Convert.ToInt16(dataRow.EstimatedTimeOfArrival.Right(2)));
				}
			}

			if (dataRow.TransportMode == Core.Constants.TransportModes.Air)
			{
				consolValue.ConsolDetail.ContainerMode = Xsd.ContainerMode.LSE;
				consolValue.ConsolDetail.ContainerModeSpecified = true;

				Xsd.FlightWithFlightNumber flightDetails = new Xsd.FlightWithFlightNumber();
				consolValue.ConsolDetail.Item = flightDetails;

				flightDetails.FlightNoJourneyNoTruckRegNo = dataRow.FinalFlightVoyage;

				consolValue.ConsolDetail.PortOfLoading.Port.Value = dataRow.DepartureCountryCode + dataRow.DepartureCity;
				consolValue.ConsolDetail.PortOfDischarge.Port.Value = dataRow.DestinationCountryCode + dataRow.DestinationCity;

				if (!dataRow.DepartureCountryCode.Equals(GlbCompany.CurrentCompany.Country.RN_Code) && dataRow.DestinationCountryCode.Equals(GlbCompany.CurrentCompany.Country.RN_Code))
				{
					if (dataRow.SailingDate.IsValid && !dataRow.SailingDate.IsEmpty)
					{
						consolValue.ConsolDetail.PortOfLoading.EstimatedDateTime = dataRow.SailingDate;
					}
					if (dataRow.EstimatedDateOfArrival.IsValid && !dataRow.EstimatedDateOfArrival.IsEmpty)
					{
						consolValue.ConsolDetail.PortOfDischarge.EstimatedDateTime = dataRow.EstimatedDateOfArrival;
					}
				}
				else
				{
					if (dataRow.EstimatedDateOfDeparture.IsValid && !dataRow.EstimatedDateOfDeparture.IsEmpty)
					{
						consolValue.ConsolDetail.PortOfLoading.EstimatedDateTime = dataRow.EstimatedDateOfDeparture;
					}
					if (dataRow.EstimatedDateOfArrival.IsValid && !dataRow.EstimatedDateOfArrival.IsEmpty)
					{
						consolValue.ConsolDetail.PortOfDischarge.EstimatedDateTime = dataRow.EstimatedDateOfArrival;
					}
				}
			}
		}

		void ProcessShipment(NIPConsolAndShipmentFlatFileDataRow dataRow)
		{
			#region defaults

			CurrentShipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = Core.Constants.PkgUnit.Package;

			#endregion

			var customValue = CurrentShipment.CustomValues.AddNew();
			customValue.Type = "IsCustomEntryOnly";
			customValue.Value = dataRow.CustomsEntryOnly;
			CurrentShipment.ShipmentDetails.TransportMode = TransportModeToXmlCodeMappings.Instance.GetExternalCode(dataRow.TransportMode, "", Context);

			ZString exemptCodes = "EXLV~EXDC~EXDD~EXLV~EXML~EXPE~EXSP~EXTI";
			if (!string.IsNullOrEmpty(dataRow.ENDExemptCode) && exemptCodes.IndexOf(dataRow.ENDExemptCode) != -1)
			{
				var customsEntryNumber = new Xsd.CustomsEntryNumber();
				customsEntryNumber.Country = Core.Constants.CountryCodes.Australia;
				customsEntryNumber.Type = dataRow.ENDExemptCode.Right(3);
				CurrentShipment.ShipmentDetails.CustomsEntryNumbers.Add(customsEntryNumber);
			}
			CurrentShipment.ShipmentDetails.PackingMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(dataRow.ServiceType, "", Context);

			Xsd.ShipmentIdentifier identifier = CurrentShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = dataRow.BLNo;

			if (dataRow.TransportMode == Core.Constants.TransportModes.Sea)
			{
				CurrentShipment.ShipmentDetails.PortOfOrigin.Port.Value = dataRow.PortOfLoadingCountry + dataRow.PortOfLoadingCity;
				CurrentShipment.ShipmentDetails.PortofDestination.Port.Value = dataRow.PortOfDeliveryCountry + dataRow.PortOfDeliveryCity;

				if (dataRow.EstimatedDateOfDeparture.IsValid && !dataRow.EstimatedDateOfDeparture.IsEmpty)
				{
					CurrentShipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = dataRow.EstimatedDateOfDeparture.AddHours(Convert.ToInt16(dataRow.EstimatedTimeOfDeparture.Left(2))).AddMinutes(Convert.ToInt16(dataRow.EstimatedTimeOfDeparture.Right(2)));
				}
				if (dataRow.EstimatedDateOfArrival.IsValid && !dataRow.EstimatedDateOfArrival.IsEmpty)
				{
					CurrentShipment.ShipmentDetails.PortofDestination.EstimatedDateTime = dataRow.EstimatedDateOfArrival.AddHours(Convert.ToInt16(dataRow.EstimatedTimeOfArrival.Left(2))).AddMinutes(Convert.ToInt16(dataRow.EstimatedTimeOfArrival.Right(2)));
				}
			}

			if (dataRow.TransportMode == Core.Constants.TransportModes.Air)
			{
				CurrentShipment.ShipmentDetails.PackingMode = Xsd.ContainerMode.LSE;
				CurrentShipment.ShipmentDetails.PortOfOrigin.Port.Value = dataRow.DepartureCountryCode + dataRow.DepartureCity;
				CurrentShipment.ShipmentDetails.PortofDestination.Port.Value = dataRow.DestinationCountryCode + dataRow.DestinationCity;

				if (!dataRow.DepartureCountryCode.Equals(GlbCompany.CurrentCompany.Country.RN_Code) && dataRow.DestinationCountryCode.Equals(GlbCompany.CurrentCompany.Country.RN_Code))
				{
					if (dataRow.SailingDate.IsValid && !dataRow.SailingDate.IsEmpty)
					{
						CurrentShipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = dataRow.SailingDate;
					}
					if (dataRow.EstimatedDateOfArrival.IsValid && !dataRow.EstimatedDateOfArrival.IsEmpty)
					{
						CurrentShipment.ShipmentDetails.PortofDestination.EstimatedDateTime = dataRow.EstimatedDateOfArrival;
					}
				}
				else
				{
					if (dataRow.EstimatedDateOfDeparture.IsValid && !dataRow.EstimatedDateOfDeparture.IsEmpty)
					{
						CurrentShipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = dataRow.EstimatedDateOfDeparture;
					}
					if (dataRow.EstimatedDateOfArrival.IsValid && !dataRow.EstimatedDateOfArrival.IsEmpty)
					{
						CurrentShipment.ShipmentDetails.PortofDestination.EstimatedDateTime = dataRow.EstimatedDateOfArrival;
					}
				}
			}

			CurrentShipment.ShipmentDetails.Consignor = new Xsd.Organisation();
			CurrentShipment.ShipmentDetails.Consignor.OwnerCode = string.Concat(dataRow.ShippersName, "_", CurrentShipment.ShipmentDetails.PortOfOrigin.Port.Value.Left(2));
			CurrentShipment.ShipmentDetails.Consignor.OrganisationDetails.Name = dataRow.ShippersName;
			Xsd.OrgAddress shippersAddress = CurrentShipment.ShipmentDetails.Consignor.OrganisationDetails.Addresses.AddNew();
			MapAddress(dataRow.ShippersAddress1, dataRow.ShippersAddress2, dataRow.ShippersAddress3, dataRow.ShippersAddress4, dataRow.ShippersAddress5, shippersAddress);

			CurrentShipment.ShipmentDetails.Consignee = new Xsd.Organisation();
			CurrentShipment.ShipmentDetails.Consignee.OwnerCode = string.Concat(dataRow.ConsigneeName, "_", CurrentShipment.ShipmentDetails.PortofDestination.Port.Value.Left(2));
			CurrentShipment.ShipmentDetails.Consignee.OrganisationDetails.Name = dataRow.ConsigneeName;
			Xsd.OrgAddress consigneeAddress = CurrentShipment.ShipmentDetails.Consignee.OrganisationDetails.Addresses.AddNew();
			MapAddress(dataRow.ConsigneeAddress1, dataRow.ConsigneeAddress2, dataRow.ConsigneeAddress3, dataRow.ConsigneeAddress4, dataRow.ConsigneeAddress5, consigneeAddress);

			CurrentShipment.ShipmentDetails.NotifyParty.Organisation = new Xsd.Organisation();
			CurrentShipment.ShipmentDetails.NotifyParty.Organisation.OwnerCode = string.Concat(dataRow.NotifyName, "_", CurrentShipment.ShipmentDetails.PortofDestination.Port.Value.Left(2));
			CurrentShipment.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name = dataRow.NotifyName;
			Xsd.OrgAddress notifyAddress = CurrentShipment.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Addresses.AddNew();
			MapAddress(dataRow.NotifyAddress1, dataRow.NotifyAddress2, dataRow.NotifyAddress3, dataRow.NotifyAddress4, dataRow.NotifyAddress5, notifyAddress);

			CurrentShipment.ShipmentDetails.Weight.Value = dataRow.GrossWeight;
			CurrentShipment.ShipmentDetails.Volume.Value = dataRow.Volume;
			CurrentShipment.ShipmentDetails.ChargeableWeight.Value = dataRow.ChargeableWeight;
			CurrentShipment.ShipmentDetails.GoodsDescription = dataRow.Goods1;
			CurrentShipment.ShipmentDetails.TotalOuterPacksQty.Value = dataRow.TotalOuterPiecesNo;
			if (!dataRow.OuterPackingType.IsEmpty)
			{
				CurrentShipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = dataRow.OuterPackingType;
			}

			CurrentShipment.ShipmentDetails.TotalInnerPacksQty.Value = dataRow.TotalInnerPiecesNo;
			CurrentShipment.ShipmentDetails.TotalInnerPacksQty.DimensionType = dataRow.InnerPackingType;

			CurrentShipment.ShipmentDetails.GoodsValue.Value = dataRow.DeclaredValue;
			CurrentShipment.ShipmentDetails.GoodsValue.CurrencyCode = dataRow.CurrencyCodeForFreight;

			CurrentShipment.ShipmentDetails.BookingReference = dataRow.JobNo;
		}

		void MapAddress(ZString addresLine1, ZString addresLine2, ZString addresLine3, ZString addresLine4, ZString addresLine5, Xsd.OrgAddress address)
		{
			if (addresLine1.IsEmpty)
			{
				address.AddressLine1 = addresLine2;
			}
			else
			{
				address.AddressLine1 = addresLine1;
				address.AddressLine2 = addresLine2;
			}
			ZStringBuilder addressCityOrSuburb = new ZStringBuilder();
			addressCityOrSuburb.AppendIfNotEmpty(addresLine3.Trim());
			addressCityOrSuburb.AppendIfNotEmpty(addresLine4.Trim());
			addressCityOrSuburb.AppendIfNotEmpty(addresLine5.Trim());
			address.CityOrSuburb = addressCityOrSuburb.ToStringWithDelimiterBetweenAppends(" ");
		}

		void ProcessContainer(Xsd.Consol consolValue, NIPConsolAndShipmentFlatFileDataRow dataRow)
		{
			Xsd.Container container = consolValue.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = dataRow.ContainerNo;
			container.Seal = dataRow.SealNo;
			container.PackingMode = consolValue.ConsolDetail.ContainerMode;
			container.ContainerType.ContainerCode = string.Concat(dataRow.ContainerFeet, dataRow.ContainerHigh, dataRow.ContainerType);

			Xsd.Package package = CurrentShipment.ShipmentDetails.Packages.AddNew();
			package.ContainerNumber = dataRow.ContainerNo;
			package.Weight.Value = dataRow.GrossWeight;
			package.Volume.Value = dataRow.Volume;
			package.NumberOfPacks = (uint)(decimal)(dataRow.TotalOuterPiecesNo);
			package.PackType = dataRow.OuterPackingType;
		}

		ValueObjectImportContext Context
		{
			get { return context = context ?? (new ValueObjectImportContext(Factory, Notification)); }
		}
		ValueObjectImportContext context;

		Xsd.Shipment CurrentShipment;
	}
}
