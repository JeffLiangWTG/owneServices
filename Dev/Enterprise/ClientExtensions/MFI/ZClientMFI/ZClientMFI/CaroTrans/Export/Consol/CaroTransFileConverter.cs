using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CoreConstants = Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans.Export
{
	public class CaroTransFileConverter : FlatFileConverter<Xsd.Consol>
	{
		public CaroTransFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		public string GetNameOfFileFromConsolDetails()
		{
			return PortInformationForFilename;
		}

		string PortInformationForFilename;
		string FileExtensionFromPort;

		protected override FlatFileDataRowCollection MapExport(Xsd.Consol consol)
		{
			if (consol.ConsolDetail != null && consol.ConsolDetail.Containers != null)
			{
				foreach (Xsd.Container container in consol.ConsolDetail.Containers)
				{
					UniqueContainerReference++;
					ContainerID = consol.ConsolDetail.AgentReference.RemoveSafe(0, 1) + UniqueContainerReference.ToString().PadLeft(2, '0');
					MapContainer(consol, container);
				}
			}
			return RowsForExport;
		}

		internal protected FlatFileDataRowCollection RowsForExport
		{
			get
			{
				if (fRowsForExport == null)
				{
					fRowsForExport = new FlatFileDataRowCollection();
				}
				return fRowsForExport;
			}
		}

		FlatFileDataRowCollection fRowsForExport;

		#region MapContainer

		protected void MapContainer(Xsd.Consol consol, Xsd.Container container)
		{
			Xsd.ShipmentCollection shipmentsForContainer = new Xsd.ShipmentCollection();
			var forwardingConsol = Factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, consol.ConsolDetail.AgentReference)).FirstOrDefault();
			var forwardingShipments = forwardingConsol.Shipments.Where(forwardingShipment => ((ForwardingShipment)forwardingShipment).OuterPackLines.
				Any(packLine => ((ForwardingPackLine)packLine).Containers.Cast<CommonContainer>().Any(commonContainer => commonContainer.JC_ContainerNum == container.ContainerNumber)));
			foreach (Xsd.Shipment shipment in consol.Shipments)
			{
				if (forwardingShipments.Cast<ForwardingShipment>().Any(forwardingShipment => forwardingShipment.JS_UniqueConsignRef == shipment.ShipmentDetails.AgentReference))
				{
					shipmentsForContainer.Add(shipment);
				}
			}

			Xsd.PackageCollection packsForContainer = new Xsd.PackageCollection();
			List<ForwardingPackLine> forwardingPackLines = new List<ForwardingPackLine>();
			foreach (ForwardingShipment forwardingShipment in forwardingShipments)
			{
				forwardingPackLines.AddRange(forwardingShipment.OuterPackLines.Where(packLine => ((ForwardingPackLine)packLine).Containers.Cast<CommonContainer>().
					Any(commonContainer => commonContainer.JC_ContainerNum == container.ContainerNumber)).Cast<ForwardingPackLine>());
			}
			foreach (Xsd.Shipment shipment in consol.Shipments)
			{
				packsForContainer.Add(GetPacksForContainer(shipment, forwardingPackLines));
			}

			FlatFileDataRow row = new FlatFileDataRow(Constants.ContainerRecord.FieldLength);

			row[Constants.ContainerRecord.RecordType] = Constants.RecordTypes.ContainerRec;
			row[Constants.ContainerRecord.ShipCode] = "MFI";
			row[Constants.ContainerRecord.ContainerId] = ContainerID;
			row[Constants.ContainerRecord.ContainerNumber] = container.ContainerNumber;
			row[Constants.ContainerRecord.OBL] = GetMasterbillFromConsol(consol.ConsolIdentifier);
			row[Constants.ContainerRecord.NumberOfBills] = shipmentsForContainer.Count.ToString();
			row[Constants.ContainerRecord.Pieces] = packsForContainer.Count.ToString();
			row[Constants.ContainerRecord.Weight] = GetWeightInKGFromPackages(packsForContainer).ToString();
			row[Constants.ContainerRecord.CBM] = GetVolumeInM3FromPackages(packsForContainer).ToString();
			MapVesselVoyageInformation(row, consol);
			MapPortOfLoading(row, consol);
			MapPortOfDischarge(row, consol);
			MapFreightRateInformation(row, shipmentsForContainer);

			if (container.ContainerType != null)
			{
				row[Constants.ContainerRecord.ContainerSize] = container.ContainerType.ISOCode;
			}

			RowsForExport.Add(row);
			MapBillsForContainer(consol, container, shipmentsForContainer);
		}

		Xsd.PackageCollection GetPacksForContainer(Xsd.Shipment shipment, IEnumerable<ForwardingPackLine> forwardingPackLines)
		{
			Xsd.PackageCollection packsForContainer = new Xsd.PackageCollection();
			foreach (Xsd.Package package in shipment.ShipmentDetails.Packages)
			{
				if (forwardingPackLines.Any(forwardingPackLine => forwardingPackLine.JL_PackageCount == package.NumberOfPacks && forwardingPackLine.JL_F3_NKPackType == package.PackType &&
					forwardingPackLine.JL_ActualWeight == package.Weight.Value && forwardingPackLine.JL_ActualWeightUQ == package.Weight.DimensionType &&
					forwardingPackLine.JL_ActualVolume == package.Volume.Value && forwardingPackLine.JL_ActualVolumeUQ == package.Volume.DimensionType))
				{
					packsForContainer.Add(package);
				}
			}

			return packsForContainer;
		}

		void MapFreightRateInformation(FlatFileDataRow row, Xsd.ShipmentCollection shipmentsForContainer)
		{
			if (shipmentsForContainer.Count > 0)
			{
				Xsd.Shipment shipment = shipmentsForContainer[0];

				if (shipment.ShipmentDetails != null && shipment.ShipmentDetails.FreightRate != null)
				{
					row[Constants.ContainerRecord.Curr] = shipment.ShipmentDetails.FreightRate.CurrencyCode;
					row[Constants.ContainerRecord.ExchRate] = shipment.ShipmentDetails.FreightRate.Value.ToString();
				}
			}
		}

		void MapVesselVoyageInformation(FlatFileDataRow row, Xsd.Consol consol)
		{
			if (consol.ConsolDetail.Item is Xsd.SailingWithVesselVoyage)
			{
				Xsd.SailingWithVesselVoyage voyageDetails = (Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item;
				row[Constants.ContainerRecord.Vessel] = voyageDetails.VesselName;
				row[Constants.ContainerRecord.Voyage] = voyageDetails.VoyageNo;
			}
		}

		void MapPortOfDischarge(FlatFileDataRow row, Xsd.Consol consol)
		{
			if (consol.ConsolDetail.PortOfDischarge != null)
			{
				Xsd.Movement portOfDischarge = consol.ConsolDetail.PortOfDischarge;

				row[Constants.ContainerRecord.PortOfDischarge] = portOfDischarge.Port.Value;
				FileExtensionFromPort = MFIDataRegistry.Instance.CaroTransExportFileExtensions.GetDescriptionFromCode(portOfDischarge.Port.Value.Left(2));

				if (portOfDischarge.EstimatedDateTime.IsValid)
				{
					row[Constants.ContainerRecord.ArrivalDate] = portOfDischarge.EstimatedDateTime.ToString("yyyyMMdd");
				}

				if (string.IsNullOrEmpty(FileExtensionFromPort))
				{
					FileExtensionFromPort = Constants.CargoTransDefaultFileExtension;
				}

				PortInformationForFilename = portOfDischarge.Port.Value.SubstringSafe(2) + ZDateTime.Now.ToString("yyMMddhhmmss") + "." + FileExtensionFromPort;
			}
		}

		void MapPortOfLoading(FlatFileDataRow row, Xsd.Consol consol)
		{
			if (consol.ConsolDetail.PortOfLoading != null)
			{
				Xsd.Movement portOfLoading = consol.ConsolDetail.PortOfLoading;
				if (portOfLoading.Port != null)
				{
					row[Constants.ContainerRecord.PortOfLoading] = portOfLoading.Port.Value;
				}

				if (portOfLoading.EstimatedDateTime.IsValid)
				{
					row[Constants.ContainerRecord.SailDate] = portOfLoading.EstimatedDateTime.ToString("yyyyMMdd");
				}
			}
		}

		#endregion

		#region Container extraction

		internal string GetMasterbillFromConsol(Xsd.ConsolIdentifierCollection identifierCollection)
		{
			string returnValue = "";
			foreach (Xsd.ConsolIdentifier identifier in identifierCollection)
			{
				if (identifier.ConsolIdentifierType == Xsd.ConsolIdentifierType.MasterWaybill)
				{
					returnValue = identifier.Value;
				}
			}
			return returnValue;
		}

		internal decimal GetWeightInKGFromPackages(Xsd.PackageCollection packages)
		{
			decimal totalWeightInKG = 0;
			foreach (Xsd.Package package in packages)
			{
				if (package != null && package.Weight.IsSpecified)
				{
					totalWeightInKG += CoreConstants.Weight.Convert(package.Weight.Value, package.Weight.DimensionType, CoreConstants.Weight.Kilograms);
				}
			}
			return totalWeightInKG;
		}

		internal decimal GetVolumeInM3FromPackages(Xsd.PackageCollection packages)
		{
			decimal totalCubicMetres = 0;
			foreach (Xsd.Package package in packages)
			{
				if (package != null && package.Volume.IsSpecified)
				{
					totalCubicMetres += CoreConstants.Volume.Convert(package.Volume.Value, package.Volume.DimensionType, CoreConstants.Volume.CubicMetres);
				}
			}
			return Utilities.Round(totalCubicMetres, 3);
		}

		#endregion

		#region Shipment Extraction (Bills)

		void MapBillsForContainer(Xsd.Consol consol, Xsd.Container container, Xsd.ShipmentCollection shipments)
		{
			foreach (Xsd.Shipment shipment in shipments)
			{
				shipment.ShipmentDetails.GoodsDescription = shipment.ShipmentDetails.GoodsDescription.Replace("\r\n", " ");
				shipment.ShipmentDetails.MarksAndNumbers = shipment.ShipmentDetails.MarksAndNumbers.Replace("\r\n", " ");

				int billBodyCount = GetMinNumberOfBillBodies(shipment.ShipmentDetails.MarksAndNumbers, shipment.ShipmentDetails.GoodsDescription);

				FlatFileDataRow row = new FlatFileDataRow(Constants.BillRecord.FieldLength);
				row[Constants.BillRecord.RecordType] = Constants.RecordTypes.BillRec;
				row[Constants.BillRecord.ContainerId] = ContainerID;
				row[Constants.BillRecord.ContainerNumber] = container.ContainerNumber;
				row[Constants.BillRecord.BOL] = GetHousebillFromShipment(shipment.ShipmentIdentifier);
				row[Constants.BillRecord.Terms] = GetShippingTerms(shipment.ShipmentDetails.Incoterm);
				row[Constants.BillRecord.NumberBillBodyLines] = billBodyCount.ToString();
				row[Constants.BillRecord.NumberChargeLines] = "0";

				if (consol.ConsolDetail.Item is Xsd.SailingWithVesselVoyage)
				{
					row[Constants.BillRecord.Vessel] = ((Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item).VesselName;
					row[Constants.BillRecord.Voyage] = ((Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item).VoyageNo;
				}

				if (shipment.ShipmentDetails.Consignor != null)
				{
					SetConsignorDetails(row, shipment.ShipmentDetails.Consignor);
				}

				if (shipment.ShipmentDetails.Consignee != null)
				{
					SetConsigneeDetails(row, shipment.ShipmentDetails.Consignee);
				}

				if (shipment.ShipmentDetails.NotifyParty != null && shipment.ShipmentDetails.NotifyParty.Organisation != null)
				{
					SetNotifyPartyDetails(row, shipment.ShipmentDetails.NotifyParty.Organisation);
				}

				RowsForExport.Add(row);

				var goodsDesc = shipment.ShipmentDetails.GoodsDescription;
				var marksAndNumbers = shipment.ShipmentDetails.MarksAndNumbers;

				for (int i = 0; i < billBodyCount; i++)
				{
					MapBillBodyForBill(i, consol, container, shipment);
				}

				shipment.ShipmentDetails.GoodsDescription = goodsDesc;
				shipment.ShipmentDetails.MarksAndNumbers = marksAndNumbers;
			}
		}

		void SetConsignorDetails(FlatFileDataRow row, Xsd.Organisation consignor)
		{
			row[Constants.BillRecord.ShipperCode] = consignor.EDICode;
			row[Constants.BillRecord.ShipperName] = consignor.OrganisationDetails.Name;
			Xsd.OrgAddress address = consignor.OrganisationDetails.Addresses.GetMainAddress();

			if (address != null)
			{
				row[Constants.BillRecord.ShipperAdd1] = address.AddressLine1.SubstringSafe(0, Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.ShipperAdd2] = address.AddressLine2.SubstringSafe(0, Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.ShipperAdd3] = address.AddressLine2.SubstringSafe(Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.ShipperCity] = address.CityOrSuburb;
				row[Constants.BillRecord.ShipperState] = address.StateOrProvince;
				row[Constants.BillRecord.ShipperZip] = address.PostCode;
				row[Constants.BillRecord.ShipperPhone] = GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Business);
				row[Constants.BillRecord.ShipperFax] = GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Fax);
			}
		}

		void SetConsigneeDetails(FlatFileDataRow row, Xsd.Organisation consignee)
		{
			row[Constants.BillRecord.ConsigneeCode] = consignee.EDICode;
			row[Constants.BillRecord.MFIAgentCode] = consignee.EDICode;
			row[Constants.BillRecord.ConsigneeName] = consignee.OrganisationDetails.Name;
			Xsd.OrgAddress address = consignee.OrganisationDetails.Addresses.GetMainAddress();

			if (address != null)
			{
				row[Constants.BillRecord.ConsigneeAdd1] = address.AddressLine1.SubstringSafe(0, Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.ConsigneeAdd2] = address.AddressLine2.SubstringSafe(0, Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.ConsigneeAdd3] = address.AddressLine2.SubstringSafe(Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.ConsigneeCity] = address.CityOrSuburb;
				row[Constants.BillRecord.ConsigneeState] = address.StateOrProvince;
				row[Constants.BillRecord.ConsigneeZip] = address.PostCode;
				row[Constants.BillRecord.ConsigneePhone] = GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Business);
				row[Constants.BillRecord.ConsigneeFax] = GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Fax);
			}
		}

		void SetNotifyPartyDetails(FlatFileDataRow row, Xsd.Organisation notifyParty)
		{
			row[Constants.BillRecord.NotifyCode] = notifyParty.EDICode;
			row[Constants.BillRecord.NotifyName] = notifyParty.OrganisationDetails.Name;
			Xsd.OrgAddress address = notifyParty.OrganisationDetails.Addresses.GetMainAddress();

			if (address != null)
			{
				row[Constants.BillRecord.NotifyAdd1] = address.AddressLine1.SubstringSafe(0, Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.NotifyAdd2] = address.AddressLine2.SubstringSafe(0, Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.NotifyAdd3] = address.AddressLine2.SubstringSafe(Constants.BillRecord.AddressLineMaxLength);
				row[Constants.BillRecord.NotifyCity] = address.CityOrSuburb;
				row[Constants.BillRecord.NotifyState] = address.StateOrProvince;
				row[Constants.BillRecord.NotifyZip] = address.PostCode;
				row[Constants.BillRecord.NotifyPhone] = GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Business);
				row[Constants.BillRecord.NotifyFax] = GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Fax);
			}
		}

		internal string GetHousebillFromShipment(Xsd.ShipmentIdentifierCollection identifierCollection)
		{
			string returnValue = "";

			foreach (Xsd.ShipmentIdentifier identifier in identifierCollection)
			{
				if (identifier.ShipmentIdentifierType == Xsd.ShipmentIdentifierType.Housebill)
				{
					returnValue = identifier.Value;
				}
			}

			return returnValue;
		}

		internal string GetShippingTerms(ZString shipmentIncoterm)
		{
			string shippingTerm = "";

			switch (shipmentIncoterm)
			{
				case Core.Constants.IncoTerms.FreeOnBoard:
				case Core.Constants.IncoTerms.ExWorks:
					shippingTerm = "P";
					break;

				default:
					shippingTerm = "C";
					break;
			}

			return shippingTerm;
		}

		internal int GetMinNumberOfBillBodies(string marksAndNumbers, string goodsDescription)
		{
			if (marksAndNumbers != null && goodsDescription != null)
			{
				int marksCount = GetNoOfRecordsRequiredForField(Constants.BillBodyRecord.MarksMaxLength, marksAndNumbers.Length);
				int goodsCount = GetNoOfRecordsRequiredForField(Constants.BillBodyRecord.DescriptionMaxLength, goodsDescription.Length);
				return (marksCount > goodsCount) ? marksCount : goodsCount;
			}
			else
			{
				return 0;
			}
		}

		int GetNoOfRecordsRequiredForField(int maxLength, int actualLength)
		{
			return (int)Math.Ceiling((double)(actualLength / (double)maxLength));
		}

		#endregion

		#region Shipment body extraction (Bill Bodys)

		internal void MapBillBodyForBill(int index, Xsd.Consol consol, Xsd.Container container, Xsd.Shipment shipment)
		{
			FlatFileDataRow row = new FlatFileDataRow(Constants.BillBodyRecord.FieldLength);
			int totalPackageCount = 0;
			decimal totalWeight = 0;
			decimal totalVolume = 0;

			// Package count, weight and volume should be output to the first '03' record
			if (index == 0)
			{
				var forwardingShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipment.ShipmentDetails.AgentReference)).FirstOrDefault();
				var forwardingPackLines = forwardingShipment.OuterPackLines.Where(packLine => ((ForwardingPackLine)packLine).Containers.Cast<CommonContainer>().Any(commonContainer => commonContainer.JC_ContainerNum == container.ContainerNumber)).Cast<ForwardingPackLine>();
				Xsd.PackageCollection packagesForContainer = GetPacksForContainer(shipment, forwardingPackLines);
				totalPackageCount = GetTotalPackageCount(packagesForContainer);
				totalWeight = GetWeightInKGFromPackages(packagesForContainer);
				totalVolume = GetVolumeInM3FromPackages(packagesForContainer);
			}

			int marksMaxLength = Constants.BillBodyRecord.MarksMaxLength;
			int descMaxLength = Constants.BillBodyRecord.DescriptionMaxLength;

			row[Constants.BillBodyRecord.RecordType] = Constants.RecordTypes.BillBodyRec;
			row[Constants.BillBodyRecord.ContainerId] = ContainerID;
			row[Constants.BillBodyRecord.ContainerNumber] = container.ContainerNumber;
			row[Constants.BillBodyRecord.BOL] = GetHousebillFromShipment(shipment.ShipmentIdentifier);
			row[Constants.BillBodyRecord.SequenceNumber] = (index + 1).ToString();

			int marksActualLength = shipment.ShipmentDetails.MarksAndNumbers.Length;
			int marksProposedLength = shipment.ShipmentDetails.MarksAndNumbers.GetStrLengthSafe(marksMaxLength);
			row[Constants.BillBodyRecord.Marks] = shipment.ShipmentDetails.MarksAndNumbers.SubstringSafe(0, marksProposedLength);
			shipment.ShipmentDetails.MarksAndNumbers = shipment.ShipmentDetails.MarksAndNumbers.SubstringSafe(marksProposedLength);
			row[Constants.BillBodyRecord.Packages] = totalPackageCount.ToString();

			int descActualLength = shipment.ShipmentDetails.GoodsDescription.Length;
			int descProposedLength = shipment.ShipmentDetails.GoodsDescription.GetStrLengthSafe(descMaxLength);
			row[Constants.BillBodyRecord.Description] = shipment.ShipmentDetails.GoodsDescription.SubstringSafe(0,descProposedLength);
			shipment.ShipmentDetails.GoodsDescription = shipment.ShipmentDetails.GoodsDescription.SubstringSafe(descProposedLength);

			row[Constants.BillBodyRecord.Weight] = decimal.Round(totalWeight, 2).ToString();
			row[Constants.BillBodyRecord.CBM] = decimal.Round(totalVolume, 3).ToString();
			RowsForExport.Add(row);
		}

		int GetTotalPackageCount(Xsd.PackageCollection packages)
		{
			int totalPackageCount = 0;
			foreach (Xsd.Package package in packages)
			{
				ZInt convertedValue = (int)package.NumberOfPacks;
				totalPackageCount += convertedValue;
			}
			return totalPackageCount;
		}

		#endregion

		#region Address extraction

		internal string GetPhoneNumberFromAddress(Xsd.OrgAddress address, Xsd.TelephoneNumberNumberType phoneNumberType)
		{
			string returnValue = "";
			foreach (Xsd.TelephoneNumber phoneNumber in address.TelephoneNumbers)
			{
				if (phoneNumber.NumberType == phoneNumberType)
				{
					returnValue = phoneNumber.Value;
				}
			}
			return returnValue;
		}

		#endregion

		int UniqueContainerReference;
		ZString ContainerID;
	}
}
