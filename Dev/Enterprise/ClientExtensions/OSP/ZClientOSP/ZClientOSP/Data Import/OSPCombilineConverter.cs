using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.OSP.Data_Import
{
	public class OSPCombilineConverter
	{
		public OSPCombilineConverter(Xsd.XmlInterchange interchange, BusinessObjectFactory factory)
		{
			this.interchange = interchange;
			this.Factory = factory;
		}

		Xsd.XmlInterchange Interchange
		{
			get
			{
				if (interchange == null)
				{
					interchange = new Xsd.XmlInterchange();
				}
				return interchange;
			}
		}
		Xsd.XmlInterchange interchange;

		public Xsd.Consol Convert(TextReader dataReader, INotifications notifications)
		{
			Xsd.Consol consol = null;

			try
			{
				string dataLine;
				while ((dataLine = dataReader.ReadLine()) != null)
				{
					IFTMIN5MBaseDataRow baseRow = new IFTMIN5MBaseDataRow(dataLine);
					switch (baseRow.SegmentCode)
					{
						case IFTMIN5MConstants.SegmentTypes.HeadingMessage:
							consol = new Xsd.Consol();
							IFTMIN5MHeadingMessageDataRow headingDataRow = new IFTMIN5MHeadingMessageDataRow(baseRow);
							Interchange.InterchangeInfo.EDIOrganisation.OwnerCode = headingDataRow.SenderID;
							break;
						case IFTMIN5MConstants.SegmentTypes.ConsolidationManifestInformation: ProcessConsolidationManifestInformation(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.ContainersOnGroupage: ProcessContainersOnGroupage(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.ShipmentsDetails: ProcessShipmentsDetails(baseRow, consol, notifications); break;
						case IFTMIN5MConstants.SegmentTypes.SeaShipmentsDetails: ProcessSeaShipmentsDetails(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.ConsignorsDetails: ProcessConsignorsDetails(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.PickUpAddress: ProcessPickUpAddress(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.ConsigneesDetails: ProcessConsigneesDetails(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.DeliveryAddresssDetails: ProcessDeliveryAddresssDetails(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.NotifyData: ProcessNotifyData(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.Goods: ProcessGoods(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.GoodsDetails: ProcessGoodsDetails(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.SeaContainers: ProcessSeaContainers(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.ShipmentsDocumentsReferences: ProcessShipmentsDocumentsReferences(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.MonetaryAmounts: ProcessMonetaryAmounts(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.AdvancedChargesAmounts: ProcessAdvancedChargesAmounts(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.Remarks: ProcessRemarks(baseRow, consol); break;
						case IFTMIN5MConstants.SegmentTypes.EndMessage: EndMessage(baseRow, consol); break;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddError("ERROR: Wrong file format!");
			}

			return consol;
		}

		void EndMessage(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			//IGNORE
		}

		void ProcessRemarks(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			//IGNORE
		}

		void ProcessAdvancedChargesAmounts(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			//IGNORE
		}

		void ProcessMonetaryAmounts(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			//IGNORE
		}

		void ProcessShipmentsDocumentsReferences(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			//IGNORE
		}

		void ProcessSeaContainers(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			//IGNORE
		}

		void ProcessGoodsDetails(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MGoodsDetailsDataRow goodsDetailsDataRow = new IFTMIN5MGoodsDetailsDataRow(baseRow);

			if (goodsDetailsDataRow.NumberOfPackages == 0 && CurrentShipment.ShipmentDetails.Packages.Count > 0)
			{
				UpdatePreviousPackage(goodsDetailsDataRow);
			}
			else
			{
				AddNewPackage(goodsDetailsDataRow);
			}
		}

		void AddNewPackage(IFTMIN5MGoodsDetailsDataRow goodsDetailsDataRow)
		{
			Xsd.Package package = CurrentShipment.ShipmentDetails.Packages.AddNew();
			package.NumberOfPacks = System.Convert.ToUInt16(goodsDetailsDataRow.NumberOfPackages);
			package.MarksAndNumbers = goodsDetailsDataRow.MarksAndNumbers;

			package.GoodsDescription = goodsDetailsDataRow.GoodsDetailsDescription1;
			if (!String.IsNullOrEmpty(goodsDetailsDataRow.GoodsDetailsDescription2))
			{
				package.GoodsDescription += String.Format("{0}{1}", System.Environment.NewLine, goodsDetailsDataRow.GoodsDetailsDescription2);
			}
			package.Weight.Value = goodsDetailsDataRow.GrossWeightKG / 1000.0m;
			package.Weight.DimensionType = Core.Constants.Weight.Kilograms;
			package.Volume.Value = goodsDetailsDataRow.Volume / 1000.0m;
			package.Volume.DimensionType = Core.Constants.Volume.CubicMetres;
			package.ContainerNumber = CurrentContainerNumber;
			package.PackType = CurrentShipment.ShipmentDetails.TotalOuterPacksQty.DimensionType;
		}

		void UpdatePreviousPackage(IFTMIN5MGoodsDetailsDataRow goodsDetailsDataRow)
		{
			Xsd.Package package = CurrentShipment.ShipmentDetails.Packages[CurrentShipment.ShipmentDetails.Packages.Count - 1];
			package.GoodsDescription += String.Format("{0}{1}", System.Environment.NewLine, goodsDetailsDataRow.GoodsDetailsDescription1);
			if (!String.IsNullOrEmpty(goodsDetailsDataRow.GoodsDetailsDescription2))
			{
				package.GoodsDescription += String.Format("{0}{1}", System.Environment.NewLine, goodsDetailsDataRow.GoodsDetailsDescription2);
			}
		}

		void ProcessGoods(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MGoodsDataRow goodsDataRow = new IFTMIN5MGoodsDataRow(baseRow);

			CurrentShipment.ShipmentDetails.GoodsDescription = goodsDataRow.GoodsDescription;
			CurrentShipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = goodsDataRow.PackageTypeCode;
			CurrentShipment.ShipmentDetails.TotalOuterPacksQty.Value += goodsDataRow.NumberOfPackages;
			CurrentShipment.ShipmentDetails.Weight.Value += goodsDataRow.GrossWeightKG / 1000.0m;
			CurrentShipment.ShipmentDetails.Weight.DimensionType = Core.Constants.Weight.Kilograms;
			CurrentShipment.ShipmentDetails.Volume.Value += goodsDataRow.Volume / 1000.0m;
			CurrentShipment.ShipmentDetails.Volume.DimensionType = Core.Constants.Volume.CubicMetres;
			CurrentShipment.ShipmentDetails.MarksAndNumbers += goodsDataRow.MarksAndNumbers + System.Environment.NewLine;
			CurrentShipment.ShipmentDetails.GoodsValue.Value += goodsDataRow.GoodsValueAmount;
			CurrentShipment.ShipmentDetails.GoodsValue.CurrencyCode = goodsDataRow.GoodsValueCurrencyCode;

			foreach (Xsd.Package package in CurrentShipment.ShipmentDetails.Packages)
			{
				package.PackType = goodsDataRow.PackageTypeCode;
			}
		}

		void ProcessNotifyData(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MOrgAndAddressDetailsDataRow notifyDataRow = new IFTMIN5MOrgAndAddressDetailsDataRow(baseRow);
			switch (notifyDataRow.Name)
			{
				case "SAME AS CNEE":
					CurrentShipment.ShipmentDetails.NotifyParty.Organisation = CurrentShipment.ShipmentDetails.Consignee;
					CurrentShipment.ShipmentDetails.NotifyParty.ContactSequenceRef = 1;
					break;
				case "SAME AS CNN":
					CurrentShipment.ShipmentDetails.NotifyParty.Organisation = CurrentShipment.ShipmentDetails.Consignor;
					CurrentShipment.ShipmentDetails.NotifyParty.ContactSequenceRef = 1;
					break;
				default:
					CurrentShipment.ShipmentDetails.NotifyParty.Organisation = new Xsd.Organisation();
					CurrentShipment.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name = notifyDataRow.Name;
					CurrentShipment.ShipmentDetails.NotifyParty.Organisation.OwnerCode = notifyDataRow.VATCode;
					Xsd.OrgAddress address = CurrentShipment.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Addresses.AddNew();
					address.AddressLine1 = notifyDataRow.Address;
					address.CityOrSuburb = notifyDataRow.City;
					address.StateOrProvince = notifyDataRow.Province;
					address.PostCode = notifyDataRow.ZIPCode;
					break;
			}
		}

		void ProcessDeliveryAddresssDetails(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MOrgAndAddressDetailsDataRow deliveryDataRow = new IFTMIN5MOrgAndAddressDetailsDataRow(baseRow);
			CurrentShipment.ShipmentDetails.Deliver.Address.CompanyName = deliveryDataRow.Name;
			CurrentShipment.ShipmentDetails.Deliver.Address.AddressLine1 = deliveryDataRow.Address;
			CurrentShipment.ShipmentDetails.Deliver.Address.CityOrSuburb = deliveryDataRow.City;
			CurrentShipment.ShipmentDetails.Deliver.Address.StateOrProvince = deliveryDataRow.Province;
			CurrentShipment.ShipmentDetails.Deliver.Address.PostCode = deliveryDataRow.ZIPCode;
		}

		void ProcessConsigneesDetails(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MOrgAndAddressDetailsDataRow consigneeDataRow = new IFTMIN5MOrgAndAddressDetailsDataRow(baseRow);
			CurrentShipment.ShipmentDetails.Consignee = new Xsd.Organisation();
			CurrentShipment.ShipmentDetails.Consignee.OrganisationDetails.Name = consigneeDataRow.Name;
			CurrentShipment.ShipmentDetails.Consignee.OwnerCode = consigneeDataRow.CustomerCode;
			Xsd.OrgAddress address = CurrentShipment.ShipmentDetails.Consignee.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = consigneeDataRow.Address;
			address.CityOrSuburb = consigneeDataRow.City;
			address.StateOrProvince = consigneeDataRow.Province;
			address.PostCode = consigneeDataRow.ZIPCode;
		}

		void ProcessPickUpAddress(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MOrgAndAddressDetailsDataRow pickupDataRow = new IFTMIN5MOrgAndAddressDetailsDataRow(baseRow);
			CurrentShipment.ShipmentDetails.Pickup.Address.CompanyName = pickupDataRow.Name;
			CurrentShipment.ShipmentDetails.Pickup.Address.AddressLine1 = pickupDataRow.Address;
			CurrentShipment.ShipmentDetails.Pickup.Address.CityOrSuburb = pickupDataRow.City;
			CurrentShipment.ShipmentDetails.Pickup.Address.StateOrProvince = pickupDataRow.Province;
			CurrentShipment.ShipmentDetails.Pickup.Address.PostCode = pickupDataRow.ZIPCode;
		}

		void ProcessConsignorsDetails(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MOrgAndAddressDetailsDataRow consignorDataRow = new IFTMIN5MOrgAndAddressDetailsDataRow(baseRow);
			CurrentShipment.ShipmentDetails.Consignor = new Xsd.Organisation();
			CurrentShipment.ShipmentDetails.Consignor.OrganisationDetails.Name = consignorDataRow.Name;
			CurrentShipment.ShipmentDetails.Consignor.OwnerCode = consignorDataRow.CustomerCode;
			Xsd.OrgAddress address = CurrentShipment.ShipmentDetails.Consignor.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = consignorDataRow.Address;
			address.CityOrSuburb = consignorDataRow.City;
			address.StateOrProvince = consignorDataRow.Province;
			address.PostCode = consignorDataRow.ZIPCode;
		}

		void ProcessSeaShipmentsDetails(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			//IGNORE
		}

		void ProcessShipmentsDetails(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol, INotifications notifications)
		{
			IFTMIN5MShipmentsDetailsDataRow shipmentDataRow = new IFTMIN5MShipmentsDetailsDataRow(baseRow);
			CurrentShipment = consol.Shipments.AddNew();
			Xsd.ShipmentIdentifier identifier = CurrentShipment.ShipmentIdentifier.AddNew();
			identifier.Value = shipmentDataRow.ShipmentNumber;
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			CurrentShipment.ShipmentDetails.Incoterm = shipmentDataRow.TermsOfDelivery;
			CurrentShipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			CurrentShipment.ShipmentDetails.PackingMode = Xsd.ContainerMode.FCL;
			CurrentShipment.ShipmentDetails.HBLIssueDate = shipmentDataRow.ShipmentDate;
			CurrentShipment.ShipmentDetails.Deliver = new Xsd.ShipmentShipmentDetailsDeliver();
			CurrentShipment.ShipmentDetails.Deliver.DeliveryFrom = shipmentDataRow.RequestedDeliveryDateTime;

			if (consol.ConsolDetail.PortOfLoadingSpecified)
			{
				CurrentShipment.ShipmentDetails.PortOfOrigin = consol.ConsolDetail.PortOfLoading;
			}
			if (consol.ConsolDetail.PortOfDischargeSpecified)
			{
				CurrentShipment.ShipmentDetails.PortofDestination = consol.ConsolDetail.PortOfDischarge;
			}
		}

		void ProcessContainersOnGroupage(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MContainersOnGroupageDataRow containerDataRow = new IFTMIN5MContainersOnGroupageDataRow(baseRow);

			Xsd.Container container = consol.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = containerDataRow.PlateContainer + containerDataRow.NumberContainer + containerDataRow.CheckDigitContainer;
			container.ContainerType.ContainerCode = containerDataRow.TypeContainer;
			container.ContainerCount = containerDataRow.ContainerProgr;
			container.Seal = containerDataRow.Seal;
			container.PackingMode = Xsd.ContainerMode.GRP;
			CurrentContainerNumber = container.ContainerNumber;
		}

		void ProcessConsolidationManifestInformation(IFTMIN5MBaseDataRow baseRow, Xsd.Consol consol)
		{
			IFTMIN5MConsolidationManifestInformationDataRow consolDataRow = new IFTMIN5MConsolidationManifestInformationDataRow(baseRow);

			Xsd.ConsolIdentifier identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.Value = consolDataRow.ManifestNumber;

			consol.ConsolDetail = new Xsd.ConsolConsolDetail();
			consol.ConsolDetail.PortOfLoading = new Xsd.Movement();
			consol.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, consolDataRow.DeparturePortCode);
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = consolDataRow.ETD;

			consol.ConsolDetail.PortOfDischarge = new Xsd.Movement();
			consol.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, consolDataRow.ArrivePortCode);
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = consolDataRow.ETA;

			Xsd.PlannedLeg leg = consol.ConsolDetail.PlannedLegs.AddNew();
			leg.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, consolDataRow.DeparturePortCode);
			leg.PortOfLoading.EstimatedDateTime = consolDataRow.ETD;

			leg.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, consolDataRow.ArrivePortCode);
			leg.PortOfDischarge.EstimatedDateTime = consolDataRow.ETA;

			Xsd.SailingForPlannedLegs item = new Xsd.SailingForPlannedLegs();
			item.VesselName = consolDataRow.CodeVessel;
			item.VoyageNo = consolDataRow.VojageNumber;
			item.ETA = consolDataRow.ETA;
			item.ETD = consolDataRow.ETD;
			leg.Item = item;

			consol.ConsolDetail.SendingAgent.OwnerCode = Interchange.InterchangeInfo.EDIOrganisation.OwnerCode;
			consol.ConsolDetail.ReceivingAgent.OwnerCode = "OSP";

			#region defaults

			consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consol.ConsolDetail.TransportModeSpecified = true;

			consol.ConsolDetail.ConsolType = Xsd.ConsolType.Agent;
			consol.ConsolDetail.ConsolTypeSpecified = true;

			consol.ConsolDetail.PaymentType = Xsd.PaymentType.CCX;
			consol.ConsolDetail.ContainerMode = Xsd.ContainerMode.GRP;

			consol.ConsolDetail.Carrier.OwnerCode = consolDataRow.SealineCode;

			#endregion
		}

		Xsd.Shipment CurrentShipment;
		ZString CurrentContainerNumber;
		readonly BusinessObjectFactory Factory;
	}
}
