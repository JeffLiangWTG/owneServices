using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;

namespace Enterprise.Client.TGE.PMS
{
	public class DataConverter : FlatFileConverter
	{
		public DataConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
		{
		}

		#region MapImport

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			fileLines.RemoveAt(0);  //Remove Header
			Consols consols = (Consols)valueObject;

			foreach (ConsolAndShipmentRecord line in fileLines)
			{
				Consol consol = new Consol();
				ProcessConsolAndShipment(consol, line);
				consols.Consol.Add(consol);
			}
		}

		#endregion

		#region ProcessConsolAndShipment

		protected void ProcessConsolAndShipment(Consol consol, ConsolAndShipmentRecord row)
		{
			ConsolIdentifier identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = ConsolIdentifierType.MasterWaybill;
			identifier.Value = row.MAWB;

			consol.ConsolDetail.TransportMode = ConsolTransportMode.AIR;
			consol.ConsolDetail.ConsolType = row.ConsolType;
			consol.ConsolDetail.ContainerMode = row.ContainerMode;
			consol.ConsolDetail.DateCreated = row.ConsolDate;

			ProcessPortsInformation(consol, row);
			ProcessFlightInformation(consol, row);
			ProcessOrganisation(consol.ConsolDetail.Creditor, row.CreditorCode);
			ProcessOrganisation(consol.ConsolDetail.Carrier, row.CarrierCode);
			ProcessOrganisation(consol.ConsolDetail.SendingAgent, row.SendingAgent);
			ProcessOrganisation(consol.ConsolDetail.ReceivingAgent, row.ReceivingAgent);
			ProcessShipment(consol, consol.Shipments.AddNew(), row);
		}

		#region ProcessPortsInformation

		protected void ProcessPortsInformation(Consol consol, ConsolAndShipmentRecord row)
		{
			consol.ConsolDetail.PortOfLoading.Port = row.PortOfLoading;
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = row.ETD;
			consol.ConsolDetail.PortOfDischarge.Port = row.PortOfDischarge;
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = row.ETA;
		}

		#endregion

		#region ProcessOrganisation

		protected void ProcessOrganisation(Organisation org, ZString code)
		{
			if (!code.IsEmpty)
			{
				org.OwnerCode = code;
				OrgAddress orgAddr = org.OrganisationDetails.Addresses.AddNew();
				orgAddr.AddressLine1 = NotSpecified;
			}
		}

		#endregion

		#endregion

		#region ProcessShipment

		protected void ProcessShipment(Consol consol, Shipment shipment, ConsolAndShipmentRecord row)
		{
			shipment.ShipmentDetails.TransportMode = TransportMode.AIR;

			ShipmentIdentifier shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = row.HAWB;

			shipment.ShipmentDetailsSpecified = true;

			shipment.ShipmentDetails.PackingMode = row.ContainerMode;
			shipment.ShipmentDetails.PortOfOrigin.Port = row.Origin;
			shipment.ShipmentDetails.PortofDestination.Port = row.Destination;

			ProcessShipperInformation(shipment, row);
			ProcessConsigneeInformation(shipment, row);

			if (row.OrderReferences.Count > 0 && !string.IsNullOrEmpty(row.OrderReferences[0]))
			{
				shipment.ShipmentDetails.OrderReferences = row.OrderReferences.ToArray();
			}

			ProcessGoodsAndPackagesInformation(shipment, row);

			CustomsEntryNumber entryNumber = shipment.ShipmentDetails.CustomsEntryNumbers.AddNew();
			entryNumber.Country = Env.CurrentCompany.Country.Code;
			entryNumber.Type = row.CAN;

			shipment.ShipmentDetails.ServiceLevel = "STD";
			shipment.ShipmentDetails.Incoterm = Core.Constants.IncoTerms.FreeOnBoard;
		}

		#endregion

		#region ProcessShipperInformation
		protected void ProcessShipperInformation(Shipment shipment, ConsolAndShipmentRecord row)
		{
			Organisation shipper = shipment.ShipmentDetails.Consignor;

			shipper.EDICode = row.ShipperCode;
			shipper.OrganisationDetails.Name = row.ShipperName;

			OrgAddress shipperAddr = shipper.OrganisationDetails.Addresses.AddNew();
			shipperAddr.AddressLine1 = row.ShipperAddressLine1;
			shipperAddr.AddressLine2 = row.ShipperAddressLine2;
			shipperAddr.CityOrSuburb = row.ShipperAddressLine3;

			shipper.OrganisationDetails.Location = row.Origin;

			TelephoneNumber shipperPhoneNo = shipperAddr.TelephoneNumbers.AddNew();
			shipperPhoneNo.Value = row.ShipperPhoneNo;
			shipperAddr.PostCode = row.ShipperPostCode;

			RegistrationNumber regNo = shipper.OrganisationDetails.RegistrationNumbers.AddNew();
			regNo.Number = row.ShipperABN;
			regNo.NumberType = RegistrationNumberTypes.GST;
			regNo.CountryOfRegistration = row.CountryOfOrigin;
		}
		#endregion

		#region ProcessConsigneeInformation
		protected void ProcessConsigneeInformation(Shipment shipment, ConsolAndShipmentRecord row)
		{
			Organisation consignee = shipment.ShipmentDetails.Consignee;
			consignee.EDICode = row.ConsigneeCode;
			consignee.OrganisationDetails.Name = row.ConsigneeName;

			OrgAddress consigneeAddr = consignee.OrganisationDetails.Addresses.AddNew();
			consigneeAddr.AddressLine1 = row.ConsigneeAddressLine1;
			consigneeAddr.AddressLine2 = row.ConsigneeAddressLine2;
			consigneeAddr.CityOrSuburb = row.ConsigneeAddressLine3;

			consigneeAddr.PostCode = row.ConsigneePostCode;
			TelephoneNumber consigneePhoneNo = consigneeAddr.TelephoneNumbers.AddNew();
			consigneePhoneNo.Value = row.ConsigneePhoneNo;

			consignee.OrganisationDetails.Location = row.ConsigneeCity;
		}
		#endregion

		#region GoodsAndPackagesInformation
		protected void ProcessGoodsAndPackagesInformation(Shipment shipment, ConsolAndShipmentRecord row)
		{
			Package pack = shipment.ShipmentDetails.Packages.AddNew();
			pack.PackType = Core.Constants.PkgUnit.Package;
			pack.NumberOfPacks = row.NumberOfPacks;
			pack.Weight.Value = row.Weight;
			pack.Weight.DimensionType = Core.Constants.Weight.Kilograms;
			pack.Volume.DimensionType = Core.Constants.Volume.CubicMetres;

			shipment.ShipmentDetails.TotalOuterPacksQty = row.TotalOuterPacks;
			shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = pack.PackType;
			shipment.ShipmentDetails.GoodsValue = row.CustomValue;
			shipment.ShipmentDetails.GoodsDescription = row.GoodsDescriptions;
			shipment.ShipmentDetails.Weight.Value = pack.Weight.Value;
			shipment.ShipmentDetails.Weight.DimensionType = pack.Weight.DimensionType;
			shipment.ShipmentDetails.Volume.DimensionType = pack.Volume.DimensionType;
		}
		#endregion

		#region FlightInformation
		protected void ProcessFlightInformation(Consol consol, ConsolAndShipmentRecord row)
		{
			FlightWithFlightNumber flight = new FlightWithFlightNumber();
			flight.FlightNoJourneyNoTruckRegNo = row.FlightCarrier + row.FlightNo;
			consol.ConsolDetail.Item = flight;
			consol.ConsolDetail.Item.ETA = row.ETA;
			consol.ConsolDetail.Item.ETD = row.ETD;
		}
		#endregion

		internal const string NotSpecified = "Not Specified";
	}
}
