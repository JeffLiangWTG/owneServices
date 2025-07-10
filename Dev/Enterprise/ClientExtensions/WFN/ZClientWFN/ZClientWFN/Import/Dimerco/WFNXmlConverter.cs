using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using BtaXsd = Enterprise.Client.WFN.Definition;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WFN
{
	public class WFNXmlConverter
	{
		public WFNXmlConverter(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		#region Convert

		public Xsd.Consol[] Convert(IValueObject[] externalValueObjects)
		{
			Xsd.Consol[] result = new Xsd.Consol[externalValueObjects.Length];

			for (int i = 0; i < externalValueObjects.Length; i++)
			{
				BtaXsd.MASTER master = (BtaXsd.MASTER)externalValueObjects[i];

				if (master != null)
				{
					Xsd.Consol consol = new Xsd.Consol();
					ProcessConsolDetails(consol, master);
					ProcessShipments(consol.Shipments, master);
					result[i] = consol;
				}
			}

			return result;
		}

		#endregion

		#region ProcessConsolDetails

		protected void ProcessConsolDetails(Xsd.Consol consol, BtaXsd.MASTER master)
		{
			Xsd.ConsolIdentifier consolId = consol.ConsolIdentifier.AddNew();
			consolId.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolId.ConsolIdentifierTypeSpecified = true;
			consolId.Value = master.MASTER_MASTERNO.Replace("-", "");

			consol.ConsolDetail = new Xsd.ConsolConsolDetail();
			Xsd.ConsolConsolDetail consolDetail = consol.ConsolDetail;
			consolDetail.ConsolType = Xsd.ConsolType.Agent;

			consol.ConsolDetail.SendingAgent = new Xsd.Organisation();
			consol.ConsolDetail.SendingAgent.OwnerCode = master.MASTER_SHPR_CODE;

			consol.ConsolDetail.PortOfLoading = new Xsd.Movement();

			RefUNLOCO loco = RefUNLOCO.LoadFromIATA(Factory, master.MASTER_DEPT);
			consol.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, loco.RL_Code);
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = GetDateTime(master.MASTER_ETD1);

			consol.ConsolDetail.PortOfDischarge = new Xsd.Movement();
			loco = RefUNLOCO.LoadFromIATA(Factory, master.MASTER_DSTN);
			consol.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, loco.RL_Code);
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = GetDateTime(master.MASTER_ETA1);

			consol.ConsolDetail.Item = new Xsd.FlightWithFlightNumber();

			if (master.MASTER_TRANSMODE == "A")
			{
				consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;
				consol.ConsolDetail.ContainerMode = Xsd.ContainerMode.LSE;
			}
			else if (master.MASTER_TRANSMODE == "O")
			{
				consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			}

			if (master.MASTER_FRT_TYPE == "P")
			{
				consol.ConsolDetail.PaymentType = Xsd.PaymentType.PPD;
			}
			else if (master.MASTER_FRT_TYPE == "C")
			{
				consol.ConsolDetail.PaymentType = Xsd.PaymentType.CCX;
			}

			consol.ConsolDetail.PlannedLegs = new Xsd.PlannedLegCollection();
			AddLeg(consol.ConsolDetail.PlannedLegs, Xsd.PlannedLegTransportType.Flight1, master);
		}

		#endregion

		#region Add Leg

		protected void AddLeg(Xsd.PlannedLegCollection legs, Xsd.PlannedLegTransportType transportType, BtaXsd.MASTER master)
		{
			ZString flightNo = master.MASTER_FLTNO1;
			if (!flightNo.IsEmpty)
			{
				ZString departure = master.MASTER_DEPT;
				ZString destination = master.MASTER_DSTN;

				Xsd.PlannedLeg leg = legs.AddNew();
				leg.TransportType = transportType;
				leg.TransportTypeSpecified = true;
				leg.TransportMode = Xsd.TransportMode.AIR;

				leg.PortOfLoading = new Xsd.Movement();
				if (!departure.IsEmpty)
				{
					RefUNLOCO unloco = RefUNLOCO.LoadFromIATA(Factory, departure);
					leg.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, unloco.RL_Code);
				}
				leg.PortOfLoading.EstimatedDateTime = GetDateTime(master.MASTER_ETD1);

				leg.PortOfDischarge = new Xsd.Movement();
				if (!destination.IsEmpty)
				{
					RefUNLOCO unloco = RefUNLOCO.LoadFromIATA(Factory, destination);
					leg.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, unloco.RL_Code);
				}
				leg.PortOfDischarge.EstimatedDateTime = GetDateTime(master.MASTER_ETA1);

				Xsd.FlightWithFlightNumber legFlight = new Xsd.FlightWithFlightNumber();
				legFlight.FlightNoJourneyNoTruckRegNo = flightNo.SubstringSafe(0, 2) + flightNo.SubstringSafe(2, 4).KeepAlphanumericCharacters();
				leg.Item = legFlight;
			}
		}

		#endregion

		#region ProcessShipments

		protected void ProcessShipments(Xsd.ShipmentCollection shipments, BtaXsd.MASTER master)
		{
			foreach (BtaXsd.HOUSE house in master.HOUSE)
			{
				Xsd.Shipment shipment = shipments.AddNew();
				shipment.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();

				Xsd.ShipmentIdentifier houseBill = shipment.ShipmentIdentifier.AddNew();
				houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
				houseBill.Value = house.HOUSE_HOUSENO.KeepAlphanumericCharacters();

				shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();

				SetOrganisationDetails(shipment.ShipmentDetails, house);
				SetPortsDetails(shipment.ShipmentDetails, house);

				shipment.ShipmentDetails.MarksAndNumbers = house.HOUSE_MARKS;
				shipment.ShipmentDetails.GoodsDescription = house.HOUSE_DESCR;

				if (master.MASTER_TRANSMODE == "A")
				{
					shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
					shipment.ShipmentDetails.PackingMode = Xsd.ContainerMode.LSE;
				}
				else if (master.MASTER_TRANSMODE == "O")
				{
					shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
				}

				if (!house.HOUSE_PONO.IsEmpty)
				{
					ZString[] orders = house.HOUSE_PONO.Split('/');
					shipment.ShipmentDetails.OrderReferences = new string[orders.Length];
					int i = 0;
					foreach (string current in orders)
					{
						shipment.ShipmentDetails.OrderReferences[i] = current;
						i++;
					}
				}

				if (master.MASTER_FRT_TYPE == "P")
				{
					shipment.ShipmentDetails.Incoterm = Core.Constants.IncoTerms.CostAndFreight;
				}
				else if (master.MASTER_FRT_TYPE == "C")
				{
					shipment.ShipmentDetails.Incoterm = Core.Constants.IncoTerms.FreeOnBoard;
				}

				SetPackageDetails(shipment.ShipmentDetails, house);
			}
		}

		#endregion

		#region GetOrgAddress

		protected Xsd.OrgAddress GetOrgAddress(ZString addr1, ZString addr2, ZString addr3, ZString phone, ZString fax, ZString postcode)
		{
			Xsd.OrgAddress address = new Xsd.OrgAddress();

			address.AddressType = Xsd.OrgAddressAddressType.MAIN;
			address.AddressLine1 = addr1;
			address.AddressLine2 = addr2;
			address.CityOrSuburb = addr3;
			address.PostCode = postcode;

			Xsd.TelephoneNumber phoneNumber = address.TelephoneNumbers.AddNew();
			phoneNumber.NumberType = Xsd.TelephoneNumberNumberType.Business;
			phoneNumber.Value = phone;

			Xsd.TelephoneNumber faxNumber = address.TelephoneNumbers.AddNew();
			faxNumber.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			faxNumber.Value = fax;

			return address;
		}

		#endregion

		#region Organisation Details

		void SetOrganisationDetails(Xsd.ShipmentShipmentDetails shipmentDetails, BtaXsd.HOUSE house)
		{
			#region Shipper

			shipmentDetails.Consignor = new Xsd.Organisation();
			shipmentDetails.Consignor.OwnerCode = house.HOUSE_SHPR_CODE;
			shipmentDetails.Consignor.OrganisationDetails.Name = house.HOUSE_SHPR_NAME;

			shipmentDetails.Consignor.OrganisationDetails.Addresses.Add(GetOrgAddress(
					house.HOUSE_SHPR_ADDR1,
					house.HOUSE_SHPR_ADDR2,
					house.HOUSE_SHPR_ADDR3,
					house.HOUSE_SHPR_PHONE,
					house.HOUSE_SHPR_FAX,
					house.HOUSE_SHPR_ZIP));

			#endregion

			#region Consignee

			shipmentDetails.Consignee = new Xsd.Organisation();
			shipmentDetails.Consignee.OwnerCode = house.HOUSE_CNEE_CODE;
			shipmentDetails.Consignee.OrganisationDetails.Name = house.HOUSE_CNEE_NAME;

			shipmentDetails.Consignee.OrganisationDetails.Addresses.Add(GetOrgAddress(
					house.HOUSE_CNEE_ADDR1,
					house.HOUSE_CNEE_ADDR2,
					house.HOUSE_CNEE_ADDR3,
					house.HOUSE_CNEE_PHONE,
					house.HOUSE_CNEE_FAX,
					house.HOUSE_CNEE_ZIP));

			#endregion

			#region Notify Party

			shipmentDetails.NotifyParty = new Xsd.ContactReference();
			shipmentDetails.NotifyParty.Organisation = new Xsd.Organisation();
			shipmentDetails.NotifyParty.Organisation.OwnerCode = house.HOUSE_NTFY_CODE;
			shipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name = house.HOUSE_NTFY_NAME;

			shipmentDetails.NotifyParty.Organisation.OrganisationDetails.Addresses.Add(GetOrgAddress(
					house.HOUSE_NTFY_ADDR1,
					house.HOUSE_NTFY_ADDR2,
					house.HOUSE_NTFY_ADDR3,
					house.HOUSE_NTFY_PHONE,
					house.HOUSE_NTFY_FAX,
					house.HOUSE_NTFY_ZIP));

			#endregion
		}

		#endregion

		#region Ports Details

		void SetPortsDetails(Xsd.ShipmentShipmentDetails shipmentDetails, BtaXsd.HOUSE house)
		{
			shipmentDetails.PortOfOrigin = new Xsd.Movement();
			shipmentDetails.PortofDestination = new Xsd.Movement();

			RefUNLOCO loco = RefUNLOCO.LoadFromIATA(Factory, house.HOUSE_DEPT);
			shipmentDetails.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(Factory, loco.RL_Code);

			loco = RefUNLOCO.LoadFromIATA(Factory, house.HOUSE_DSTN);
			shipmentDetails.PortofDestination.Port = Xsd.UNLOCO.FromPortCode(Factory, loco.RL_Code);

			if (!house.HOUSE_ETD.IsEmpty)
			{
				shipmentDetails.PortOfOrigin.EstimatedDateTime = GetDateTime(house.HOUSE_ETD);
			}
			else if (!house.HOUSE_ATD.IsEmpty)
			{
				shipmentDetails.PortOfOrigin.EstimatedDateTime = GetDateTime(house.HOUSE_ATD);
			}

			if (!house.HOUSE_ETA.IsEmpty)
			{
				shipmentDetails.PortofDestination.EstimatedDateTime = GetDateTime(house.HOUSE_ETA);
			}
			else if (!house.HOUSE_ATA.IsEmpty)
			{
				shipmentDetails.PortofDestination.EstimatedDateTime = GetDateTime(house.HOUSE_ATA);
			}
		}

		#endregion

		#region Packs Details

		void SetPackageDetails(Xsd.ShipmentShipmentDetails shipmentDetails, BtaXsd.HOUSE house)
		{
			shipmentDetails.Weight = new Xsd.DimensionValue();
			shipmentDetails.ChargeableWeight = new Xsd.DimensionValue();
			shipmentDetails.Volume = new Xsd.DimensionValue();

			shipmentDetails.Weight.Value = ZDecimal.ParseSafe(house.HOUSE_GWT, 0.00);
			shipmentDetails.ChargeableWeight.Value = ZDecimal.ParseSafe(house.HOUSE_CWT, 0.00);
			shipmentDetails.Volume.Value = ZDecimal.ParseSafe(house.HOUSE_VWT, 0.00);
			shipmentDetails.Volume.DimensionType = Core.Constants.Volume.CubicMetres;

			if (house.HOUSE_WTUOM == "L")
			{
				shipmentDetails.Weight.DimensionType = Core.Constants.Weight.Pounds;
			}
			else if (house.HOUSE_WTUOM == "K")
			{
				shipmentDetails.Weight.DimensionType = Core.Constants.Weight.Kilograms;
			}

			ZDecimal numberOfPacks = ZDecimal.ParseSafe(house.HOUSE_PCS, ZDecimal.Zero);
			shipmentDetails.TotalOuterPacksQty.Value = numberOfPacks;
			shipmentDetails.TotalOuterPacksQty.DimensionType = Core.Constants.PkgUnit.Package;

			Xsd.Package package = shipmentDetails.Packages.AddNew();

			uint packs;
			uint.TryParse(house.HOUSE_PCS, out packs);
			package.NumberOfPacks = packs;
			package.PackType = Core.Constants.PkgUnit.Package;
			package.Weight.Value = ZDecimal.ParseSafe(house.HOUSE_GWT, 0.00);
			package.Weight.DimensionType = shipmentDetails.Weight.DimensionType;
			package.Volume.Value = ZDecimal.ParseSafe(house.HOUSE_VWT, 0.00);
			package.Volume.DimensionType = Core.Constants.Volume.CubicMetres;
		}

		#endregion

		#region Parse

		ZDateTime GetDateTime(ZString xmlDateTimeString)
		{
			ZDateTime result = new ZDateTime();

			if (!ZDateTime.TryParseExact(xmlDateTimeString, out result, "yyyyMMdd"))
			{
				result = ZDateTime.Empty;
			}
			return result;
		}

		#endregion

		readonly BusinessObjectFactory Factory;
	}
}
