using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using TELXsd = Enterprise.Client.TEL.Definition;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.TEL.Import
{
	class TELConsolShipXmlConverter
	{
		public TELConsolShipXmlConverter()
		{
		}

		#region Convert

		public Xsd.Consol[] Convert(IValueObject[] externalValueObjects)
		{
			Xsd.Consol[] result = new Xsd.Consol[externalValueObjects.Length];
			for (int i = 0; i < externalValueObjects.Length; i++)
			{
				TELXsd.SeaMasterBills manifest = (TELXsd.SeaMasterBills)externalValueObjects[i];
				Xsd.Consol consolValue = new Xsd.Consol();

				foreach (TELXsd.SeaMasterBillsMasterBill masterbill in manifest.MasterBill)
				{
					MapConsolDetails(consolValue, masterbill);
				}
				MapOuterPacks(consolValue);
				result[i] = consolValue;
			}
			return result;
		}

		#endregion

		#region Implementation

		readonly ZString AddressNotSpecified = "UNSPECIFIED";

		void MapConsolDetails(Xsd.Consol xsdConsol, TELXsd.SeaMasterBillsMasterBill telXsdMasterbill)
		{
			Xsd.ConsolIdentifier identifier = xsdConsol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = telXsdMasterbill.MasterBillNr;

			xsdConsol.ConsolDetail.ConsolType = Xsd.ConsolType.Agent;
			xsdConsol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			xsdConsol.ConsolDetail.TransportModeSpecified = true;

			#region Organisation
			xsdConsol.ConsolDetail.SendingAgent.OwnerCode = telXsdMasterbill.ShiCompID;
			xsdConsol.ConsolDetail.SendingAgent.OrganisationDetails.Name = telXsdMasterbill.Shipper;
			SetAddressIfAddressIsValid(xsdConsol.ConsolDetail.SendingAgent.OrganisationDetails.Addresses, telXsdMasterbill.ShiAddr1, telXsdMasterbill.ShiAddr2, telXsdMasterbill.ShiAddr3);

			xsdConsol.ConsolDetail.ReceivingAgent.OwnerCode = telXsdMasterbill.ConCompID;
			xsdConsol.ConsolDetail.ReceivingAgent.OrganisationDetails.Name = telXsdMasterbill.Consignee;
			SetAddressIfAddressIsValid(xsdConsol.ConsolDetail.ReceivingAgent.OrganisationDetails.Addresses, telXsdMasterbill.ConAddr1, telXsdMasterbill.ConAddr2, telXsdMasterbill.ConAddr3);

			xsdConsol.ConsolDetail.Carrier.OwnerCode = telXsdMasterbill.SliCompID;
			xsdConsol.ConsolDetail.Carrier.OrganisationDetails.Name = telXsdMasterbill.ShipLiner;
			SetAddressIfAddressIsValid(xsdConsol.ConsolDetail.Carrier.OrganisationDetails.Addresses, AddressNotSpecified, AddressNotSpecified, AddressNotSpecified);
			#endregion

			xsdConsol.ConsolDetail.PaymentType = telXsdMasterbill.SeaFrg.Contains("COLLECT") ? Xsd.PaymentType.CCX : Xsd.PaymentType.PPD;

			xsdConsol.ConsolDetail.PortOfLoading.Port.Value = telXsdMasterbill.PortLoadCode;
			xsdConsol.ConsolDetail.PortOfDischarge.Port.Value = telXsdMasterbill.PortDiscCode;

			xsdConsol.ConsolDetail.PlannedLegs = new Xsd.PlannedLegCollection();
			Xsd.PlannedLeg leg = xsdConsol.ConsolDetail.PlannedLegs.AddNew();
			leg.TransportType = Xsd.PlannedLegTransportType.MainVessel;
			leg.TransportTypeSpecified = true;
			leg.TransportMode = Xsd.TransportMode.SEA;
			leg.PortOfLoading.Port.Value = telXsdMasterbill.PortLoadCode;
			leg.PortOfDischarge.Port.Value = telXsdMasterbill.PortDiscCode;

			Xsd.SailingForPlannedLegs item = new Xsd.SailingForPlannedLegs();
			item.VesselName = telXsdMasterbill.Vessel;
			item.VoyageNo = telXsdMasterbill.Voyage;
			leg.Item = item;

			foreach (TELXsd.SeaMasterBillsMasterBillContainer telXsdContainer in telXsdMasterbill.ContainerLoadings)
			{
				MapContainerDetails(xsdConsol.ConsolDetail.Containers.AddNew(), telXsdContainer);
			}

			if (xsdConsol.ConsolDetail.Containers.Count > 0)
			{
				xsdConsol.ConsolDetail.ContainerMode = xsdConsol.ConsolDetail.Containers[0].PackingMode;
			}

			foreach (TELXsd.SeaMasterBillsMasterBillHouseBill telXsdHousebill in telXsdMasterbill.HouseBills)
			{
				Xsd.Shipment xsdShipment = xsdConsol.Shipments.AddNew();
				MapShipmentDetails(xsdConsol, xsdShipment, telXsdHousebill);
				xsdShipment.ShipmentDetails.PackingMode = xsdConsol.ConsolDetail.ContainerMode;
			}
		}

		void MapShipmentDetails(Xsd.Consol xsdConsol, Xsd.Shipment xsdShipment, TELXsd.SeaMasterBillsMasterBillHouseBill telXsdHousebill)
		{
			Xsd.ShipmentIdentifier identifier = xsdShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = telXsdHousebill.BilladNr;

			Xsd.ShipmentShipmentDetails shipmentDetails = xsdShipment.ShipmentDetails;

			#region Organisations

			shipmentDetails.Consignor.OwnerCode = telXsdHousebill.ShiCompID;
			shipmentDetails.Consignor.OrganisationDetails.Name = telXsdHousebill.Shipper;
			SetAddressIfAddressIsValid(shipmentDetails.Consignor.OrganisationDetails.Addresses, telXsdHousebill.ShiAddr1, telXsdHousebill.ShiAddr2, telXsdHousebill.ShiAddr3);

			shipmentDetails.Consignee.OwnerCode = telXsdHousebill.ConCompID;
			shipmentDetails.Consignee.OrganisationDetails.Name = telXsdHousebill.Consignee;
			SetAddressIfAddressIsValid(shipmentDetails.Consignee.OrganisationDetails.Addresses, telXsdHousebill.ConAddr1, telXsdHousebill.ConAddr2, telXsdHousebill.ConAddr3);

			shipmentDetails.NotifyParty.Organisation.OwnerCode = telXsdHousebill.NotCompID;
			shipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name = telXsdHousebill.Notify;
			SetAddressIfAddressIsValid(shipmentDetails.NotifyParty.Organisation.OrganisationDetails.Addresses, telXsdHousebill.NotAddr1, telXsdHousebill.NotAddr2, telXsdHousebill.NotAddr3);

			shipmentDetails.Deliver.DeliveryAgent.OwnerCode = telXsdHousebill.OsaCompID;
			shipmentDetails.Deliver.DeliveryAgent.OrganisationDetails.Name = telXsdHousebill.OsAgent;
			SetAddressIfAddressIsValid(shipmentDetails.Deliver.DeliveryAgent.OrganisationDetails.Addresses, telXsdHousebill.OsaAddr1, telXsdHousebill.OsaAddr2, telXsdHousebill.OsaAddr3);

			#endregion

			shipmentDetails.HBLIssueDate = GetDate(telXsdHousebill.DateIssue, false);
			shipmentDetails.ShippedOnBoardDate = GetDate(telXsdHousebill.DateOnBoard, false);

			shipmentDetails.PortOfOrigin.Port.Value = telXsdHousebill.OriginPortCode;
			shipmentDetails.PortofDestination.Port.Value = telXsdHousebill.PortDiscCode;

			shipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentDetails.Incoterm = telXsdHousebill.SeaFrg.Contains("COLLECT") ? Core.Constants.IncoTerms.FreeOnBoard : Core.Constants.IncoTerms.CarriagePaidTo;

			shipmentDetails.GoodsDescription = telXsdHousebill.BillText;

			foreach (TELXsd.SeaMasterBillsMasterBillHouseBillHBContainer packing in telXsdHousebill.HBContainerLoadings)
			{
				MapPackageDetails(xsdConsol, xsdShipment.ShipmentDetails.Packages.AddNew(), packing);
			}
		}

		void MapContainerDetails(Xsd.Container xsdContainer, TELXsd.SeaMasterBillsMasterBillContainer telXsdContainer)
		{
			xsdContainer.Seal = telXsdContainer.SealNr;
			xsdContainer.PackingMode = (Xsd.ContainerMode)Enum.Parse(typeof(Xsd.ContainerMode), telXsdContainer.LoadingType);
			xsdContainer.ContainerNumber = telXsdContainer.ContNr;

			if (!telXsdContainer.ContSize.IsEmpty && !telXsdContainer.ContrType.IsEmpty)
			{
				ZString containerCode = telXsdContainer.ContSize.SubstringSafe(0, 2) + telXsdContainer.ContrType;
				xsdContainer.ContainerType.ContainerCode = containerCode;
			}
		}

		void MapOuterPacks(Xsd.Consol xsdConsol)
		{
			foreach (Xsd.Shipment xsdShipment in xsdConsol.Shipments)
			{
				ZString outerPackageType = ZString.Empty;

				foreach (Xsd.Package xsdPackage in xsdShipment.ShipmentDetails.Packages)
				{
					if (outerPackageType.IsEmpty)
					{
						outerPackageType = xsdPackage.PackType;
					}

					xsdShipment.ShipmentDetails.TotalOuterPacksQty.Value += xsdPackage.NumberOfPacks;
					xsdShipment.ShipmentDetails.Volume.Value += xsdPackage.Volume.Value;
					xsdShipment.ShipmentDetails.Weight.Value += xsdPackage.Weight.Value;
				}

				if (!outerPackageType.IsEmpty)
				{
					xsdShipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = outerPackageType;
				}
				xsdShipment.ShipmentDetails.Volume.DimensionType = Core.Constants.Volume.CubicMetres;
				xsdShipment.ShipmentDetails.Weight.DimensionType = Core.Constants.Weight.Kilograms;
			}
		}

		protected void MapPackageDetails(Xsd.Consol xsdConsol, Xsd.Package xsdPackage, TELXsd.SeaMasterBillsMasterBillHouseBillHBContainer telXsdPacking)
		{
			xsdPackage.ContainerNumber = telXsdPacking.ContNr;

			uint quantity;
			uint.TryParse(telXsdPacking.NrPkg, out quantity);
			xsdPackage.NumberOfPacks = quantity;
			xsdPackage.PackType = telXsdPacking.TypePkg;

			ZDecimal value;
			ZDecimal.TryParse(telXsdPacking.Gw, out value);
			xsdPackage.Weight.Value = value;
			xsdPackage.Weight.DimensionType = Core.Constants.Weight.Kilograms;

			value = ZDecimal.Zero;
			ZDecimal.TryParse(telXsdPacking.Cbm, out value);
			xsdPackage.Volume.Value = value;
			xsdPackage.Volume.DimensionType = Core.Constants.Volume.CubicMetres;

			if (!telXsdPacking.OriServ.IsEmpty && !telXsdPacking.DstServ.IsEmpty && !telXsdPacking.ContNr.IsEmpty)
			{
				Xsd.Container xsdContainer = xsdConsol.ConsolDetail.Containers.GetContainerFromCollection(telXsdPacking.ContNr);
				if (xsdContainer != null && xsdContainer.DeliveryMode.IsEmpty)
				{
					xsdContainer.DeliveryMode = telXsdPacking.OriServ + "/" + telXsdPacking.DstServ;
				}
			}
		}

		void SetAddressIfAddressIsValid(Xsd.OrgAddressCollection orgAddressCollection, ZString addr1, ZString addr2, ZString addr3)
		{
				Xsd.OrgAddress address = orgAddressCollection.Count > 0 ? orgAddressCollection[0] : orgAddressCollection.AddNew();
				address.AddressLine1 = addr1.IsEmpty ? AddressNotSpecified : addr1;
				address.AddressLine2 = addr2.IsEmpty ? AddressNotSpecified : addr2;
				address.CityOrSuburb = addr3.IsEmpty ? AddressNotSpecified : addr3;
				address.AddressType = Xsd.OrgAddressAddressType.MAIN;
		}

		#region TELDateTimeProvider

		ZDateTime GetDate(String dateString, bool usingCustomProvider)
		{
			DateTime date;
			DateTime.TryParse(dateString, out date);
			if (usingCustomProvider)
			{
				DateTime dateTemp;
				DateTime.TryParse(dateString, new TELDateTimeProvider(), DateTimeStyles.None, out dateTemp);
				date = dateTemp;
			}
			return date;
		}

		class TELDateTimeProvider : IFormatProvider
		{
			object IFormatProvider.GetFormat(Type formatType)
			{
				DateTimeFormatInfo info = new DateTimeFormatInfo();
				info.FullDateTimePattern = "yyyy-mm-dd";
				return info;
			}
		}

		#endregion

		#endregion
	}
}
