using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoFlatFileConverter : FlatFileConverter
	{
		public SeaCargoFlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection rows)
		{
			if (IsDataValid(rows))
			{
				Xsd.Consol oceanBill = valueObject as Xsd.Consol;
				PopulateOceanBill(oceanBill, rows[0]);

				for (int i = 1; i <= rows.Count - 1; i++)
				{
					FlatFileDataRow row = rows[i];
					switch (row[0])
					{
						case ContainerQualifier:
							if (RowContainsMinimumFields(row, ContainerColumns.MinimumFieldCount, i + 1, Notification))
							{
								Xsd.Container container = oceanBill.ConsolDetail.Containers.AddNew();
								PopulateContainer(container, row);
							}
							break;

						case HouseBillQualifier:
							if (RowContainsMinimumFields(row, HouseBillColumns.MinimumFieldCount, i + 1, Notification))
							{
								Xsd.Shipment houseBill = oceanBill.Shipments.AddNew();
								PopulateHouseBill(houseBill, row);
							}
							break;

						case PackingQualifier:
							Xsd.Shipment relatedHouseBill = FindHouseBill(oceanBill.Shipments, row[PackingColumns.HouseBill]);
							if (relatedHouseBill != null && RowContainsMinimumFields(row, PackingColumns.MinimumFieldCount, i + 1, Notification))
							{
								Xsd.Package package = relatedHouseBill.ShipmentDetails.Packages.AddNew();
								PopulatePacking(relatedHouseBill, package, row);
							}
							break;

						default:
							ZString fileFormatErrorMsg = String.Format("Unknown qualifier detected. Expected {0}, {1} or {2} but found {3} at line {4}.", ContainerQualifier, HouseBillQualifier, PackingQualifier, row[0], i + 1);
							Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, fileFormatErrorMsg));
							break;
					}
				}
			}
			else
			{
				string corruptDataField = rows.Count > 0 && rows[0].FieldCount > 0 ? rows[0].GetField(0).ToString() : "no data";
				ZString fileFormatErrorMsg = String.Format("Corrupt or invalid Sea Cargo CSV file. Expected {0} but found {1} at line 1.", OceanBillQualifier, corruptDataField);
				Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, fileFormatErrorMsg));
			}
		}

		#region RowContainsMinimumFields

		bool RowContainsMinimumFields(FlatFileDataRow row, int minimumFieldCount, int lineNumber, INotifications notifier)
		{
			bool result = true;
			if (row.FieldCount < minimumFieldCount)
			{
				notifier.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, String.Format("Expected at least {0} fields in line {1} but found only {2}.", minimumFieldCount, lineNumber, row.FieldCount)));
				result = false;
			}
			return result;
		}
		#endregion

		#region OceanBill

		void PopulateOceanBill(Xsd.Consol oceanBill, FlatFileDataRow row)
		{
			Xsd.ConsolIdentifier id = oceanBill.ConsolIdentifier.AddNew();
			id.Value = row[OceanBillColumns.OceanBill];
			id.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;

			if (!row[OceanBillColumns.MasterHouse].IsEmpty)
			{
				Xsd.ConsolIdentifier masterHouse = oceanBill.ConsolIdentifier.AddNew();
				masterHouse.Value = row[OceanBillColumns.MasterHouse];
				masterHouse.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			}
			oceanBill.ConsolDetail.PortOfLoading.Port.Value = row[OceanBillColumns.PortOfLoading];
			oceanBill.ConsolDetail.PortOfDischarge.Port.Value = row[OceanBillColumns.PortOfDischarge];
			oceanBill.ConsolDetail.PortFirstArrival.Port.Value = row[OceanBillColumns.PortOfFirstArrival];
			Xsd.SailingWithVesselVoyage voyage = new Xsd.SailingWithVesselVoyage();
			if (voyage != null)
			{
				voyage.VesselName = row[OceanBillColumns.Vessel];
				voyage.LloydsNo = row[OceanBillColumns.VesselLloyds];
				voyage.VoyageNo = row[OceanBillColumns.Voyage];

				ZDateTime arrivalDate;
				if (ZDateTime.TryParseExact(row[OceanBillColumns.DateOfArrival], out arrivalDate, "yyyyMMdd"))
				{
					voyage.ETA = arrivalDate;
				}
				oceanBill.ConsolDetail.Item = voyage;
			}

			if (!row[OceanBillColumns.PrincipalCoRegOrCustomsCode].IsEmpty && !row[OceanBillColumns.PrincipalCountryCode].IsEmpty)
			{
				Xsd.Organisation principal = new Xsd.Organisation();
				principal.IsSpecified = ZBool.True;
				principal.OrganisationDetails = new Xsd.OrganisationDetail();
				Xsd.RegistrationNumber aBN = principal.OrganisationDetails.RegistrationNumbers.AddNew();
				aBN.NumberType = Xsd.RegistrationNumberTypes.GST;
				aBN.CountryOfRegistration = row[OceanBillColumns.PrincipalCountryCode];
				aBN.Number = row[OceanBillColumns.PrincipalCoRegOrCustomsCode];
				oceanBill.ConsolDetail.ReceivingAgent = principal;
			}
		}

		bool IsDataValid(FlatFileDataRowCollection rows)
		{
			return rows.Count > 0 && rows[0].FieldCount > 0 && (rows[0].GetField(0) == OceanBillQualifier);
		}

		#endregion

		#region Container

		void PopulateContainer(Xsd.Container containerXsd, FlatFileDataRow row)
		{
			containerXsd.ContainerNumber = row[ContainerColumns.ContainerNumber];
			containerXsd.Seal = row[ContainerColumns.SealNumber];

			ZString rC_Code = row[ContainerColumns.ContainerType];
			ZString iSOType = row[ContainerColumns.ContainerSizeOrISOCode];
			ZString containerType = row[ContainerColumns.TypeOfContainer];

			if (!rC_Code.IsEmpty && (iSOType.IsEmpty || containerType.IsEmpty))
			{
				var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, rC_Code);
				if (container != null)
				{
					CMRContainerUtilities containerUtility = new CMRContainerUtilities();
					iSOType = container.RC_ISOType;
					containerType = container.RC_ContainerType;
				}
			}
			containerXsd.ContainerType.ISOCode = row[ContainerColumns.ContainerSizeOrISOCode];
			containerXsd.ContainerType.USContainerCode = row[ContainerColumns.TypeOfContainer];

			containerXsd.IsShipperOwnedContainer = GetValueFromBooleanField(row, ContainerColumns.IsShipperOwnedContainer);
			if (!row[ContainerColumns.ContainerMode].IsEmpty)
			{
				containerXsd.PackingMode = GetContainerModeFromString(row[ContainerColumns.ContainerMode]);
			}
		}

		Xsd.ContainerMode GetContainerModeFromString(string code)
		{
			if (code.ToUpper() == nameof(Xsd.ContainerMode.LCL))
			{
				return Xsd.ContainerMode.LCL;
			}
			else if (code.ToUpper() == nameof(Xsd.ContainerMode.FCL))
			{
				return Xsd.ContainerMode.FCL;
			}
			else if (code.ToUpper() == nameof(Xsd.ContainerMode.FCX))
			{
				return Xsd.ContainerMode.FCX;
			}
			else if (code.ToUpper() == nameof(Xsd.ContainerMode.BBK))
			{
				return Xsd.ContainerMode.BBK;
			}
			else if (code.ToUpper() == nameof(Xsd.ContainerMode.BLK))
			{
				return Xsd.ContainerMode.BLK;
			}
			else
			{
				throw new Exception("Unknown Container Mode");
			}
		}

		#endregion

		#region HouseBill

		void PopulateHouseBill(Xsd.Shipment houseBillXsd, FlatFileDataRow row)
		{
			Xsd.ShipmentIdentifier id = houseBillXsd.ShipmentIdentifier.AddNew();
			id.Value = row[OceanBillColumns.OceanBill];
			id.ShipmentIdentifierType = Enterprise.DataTransfer.Xml.XsdVersion1.ShipmentIdentifierType.Housebill;

			if (!row[HouseBillColumns.MasterHouseBill].IsEmpty)
			{
				Xsd.ShipmentIdentifier masterHouseBill = houseBillXsd.ShipmentIdentifier.AddNew();
				masterHouseBill.Value = row[HouseBillColumns.MasterHouseBill];
				masterHouseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.CoLoadMaster;
			}

			houseBillXsd.ShipmentDetails.PortOfOrigin.Port.Value = row[HouseBillColumns.PortOfOrigin];
			houseBillXsd.ShipmentDetails.PortofDestination.Port.Value = row[HouseBillColumns.PortOfDestination];
			PopulateDeclaration(houseBillXsd.Declaration, row);
			PopulateConsignor(houseBillXsd, row);
			PopulateConsignee(houseBillXsd, row);
			PopulateNotifyParty(houseBillXsd, row);
		}

		void PopulateDeclaration(Xsd.Declaration declarationXsd, FlatFileDataRow row)
		{
			declarationXsd.GoodsOrigin = row[HouseBillColumns.GoodsOrigin];
			declarationXsd.PaymentTerms = row[HouseBillColumns.PrepaidCollectOther];
		}

		void PopulateConsignor(Xsd.Shipment houseBillXsd, FlatFileDataRow row)
		{
			houseBillXsd.ShipmentDetails.Consignor.OrganisationDetails.Name = row[HouseBillColumns.ConsignorName];
			Xsd.OrgAddress mainAddress = houseBillXsd.ShipmentDetails.Consignor.OrganisationDetails.Addresses.AddNew();
			mainAddress.AddressLine1 = row[HouseBillColumns.ConsignorAddress1];
			mainAddress.AddressLine2 = row[HouseBillColumns.ConsignorAddress2];
			mainAddress.CityOrSuburb = row[HouseBillColumns.ConsignorSuburb];
			mainAddress.PostCode = row[HouseBillColumns.ConsignorPostCode];

			// Detect if imported file has telephone/country swapped

			int consignorPhoneColumn = HouseBillColumns.ConsignorPhone;
			int consignorFaxColumn = HouseBillColumns.ConsignorFax;
			int consignorCountryColumn = HouseBillColumns.ConsignorCountryCode;
			int consignorCustomsRegColumn = HouseBillColumns.ConsigneeCoRegOrCustomsCode;

			if (row[HouseBillColumns.ConsignorCountryCode].Replace(" ", "").IsNumbersOnlyOrEmpty && row[HouseBillColumns.ConsignorPhone].Replace(" ", "").IsLettersOnlyOrEmpty)
			{
				consignorCountryColumn = HouseBillColumns.ConsignorPhone;
				consignorCustomsRegColumn = HouseBillColumns.ConsignorFax;
				consignorPhoneColumn = HouseBillColumns.ConsignorCountryCode;
				consignorFaxColumn = HouseBillColumns.ConsignorCoRegOrCustomsCode;
			}

			Xsd.TelephoneNumber fax = mainAddress.TelephoneNumbers.AddNew();
			fax.Value = row[consignorFaxColumn];
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			Xsd.TelephoneNumber phone = mainAddress.TelephoneNumbers.AddNew();
			phone.Value = row[consignorPhoneColumn];
			phone.NumberType = Xsd.TelephoneNumberNumberType.Business;

			if (!row[consignorCountryColumn].IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryCode(Factory, row[consignorCountryColumn]);
				if (country != null)
				{
					houseBillXsd.ShipmentDetails.Consignor.OrganisationDetails.Location = new Xsd.UNLOCO();
					houseBillXsd.ShipmentDetails.Consignor.OrganisationDetails.Location.Country = country.RN_DescMultilingual;
					if (!row[consignorCustomsRegColumn].IsEmpty)
					{
						Xsd.RegistrationNumber aBN = houseBillXsd.ShipmentDetails.Consignor.OrganisationDetails.RegistrationNumbers.AddNew();
						aBN.NumberType = Xsd.RegistrationNumberTypes.GST;
						aBN.CountryOfRegistration = country.RN_Code;
						aBN.Number = row[consignorCustomsRegColumn];
					}
				}
			}
		}

		void PopulateConsignee(Xsd.Shipment houseBillXsd, FlatFileDataRow row)
		{
			houseBillXsd.ShipmentDetails.Consignee.OrganisationDetails.Name = row[HouseBillColumns.ConsigneeName];
			Xsd.OrgAddress mainAddress = houseBillXsd.ShipmentDetails.Consignee.OrganisationDetails.Addresses.AddNew();
			mainAddress.AddressLine1 = row[HouseBillColumns.ConsigneeAddress1];
			mainAddress.AddressLine2 = row[HouseBillColumns.ConsigneeAddress2];
			mainAddress.CityOrSuburb = row[HouseBillColumns.ConsigneeSuburb];
			mainAddress.PostCode = row[HouseBillColumns.ConsigneePostCode];
			Xsd.TelephoneNumber fax = mainAddress.TelephoneNumbers.AddNew();
			fax.Value = row[HouseBillColumns.ConsigneeFax];
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			Xsd.TelephoneNumber phone = mainAddress.TelephoneNumbers.AddNew();
			phone.Value = row[HouseBillColumns.ConsigneePhone];
			phone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			if (!row[HouseBillColumns.ConsigneeCountryCode].IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryCode(Factory, row[HouseBillColumns.ConsigneeCountryCode]);
				if (country != null)
				{
					houseBillXsd.ShipmentDetails.Consignee.OrganisationDetails.Location = new Xsd.UNLOCO();
					houseBillXsd.ShipmentDetails.Consignee.OrganisationDetails.Location.Country = country.RN_DescMultilingual;
				}
			}
		}

		void PopulateNotifyParty(Xsd.Shipment houseBillXsd, FlatFileDataRow row)
		{
			houseBillXsd.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name = row[HouseBillColumns.NotifyName];
			Xsd.OrgAddress mainAddress = houseBillXsd.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Addresses.AddNew();
			mainAddress.AddressLine1 = row[HouseBillColumns.NotifyAddress1];
			mainAddress.AddressLine2 = row[HouseBillColumns.NotifyAddress2];
			mainAddress.CityOrSuburb = row[HouseBillColumns.NotifySuburb];
			mainAddress.PostCode = row[HouseBillColumns.NotifyPostCode];
			Xsd.TelephoneNumber fax = mainAddress.TelephoneNumbers.AddNew();
			fax.Value = row[HouseBillColumns.NotifyFax];
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			Xsd.TelephoneNumber phone = mainAddress.TelephoneNumbers.AddNew();
			phone.Value = row[HouseBillColumns.NotifyPhone];
			phone.NumberType = Xsd.TelephoneNumberNumberType.Business;

			if (!row[HouseBillColumns.NotifyCountryCode].IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryCode(Factory, row[HouseBillColumns.NotifyCountryCode]);
				if (country != null)
				{
					houseBillXsd.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Location = new Xsd.UNLOCO();
					houseBillXsd.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Location.Country = country.RN_DescMultilingual;
				}
			}
		}

		#endregion

		#region Packing

		void PopulatePacking(Xsd.Shipment houseBillXsd, Xsd.Package packageXsd, FlatFileDataRow row)
		{
			packageXsd.ContainerNumber = row[PackingColumns.ContainerNumber];
			ZDecimal packs = row.GetFieldAsZDecimal(PackingColumns.PackageCount);
			if (packs < 0)
			{
				string msg = "The number of packs cannot be negative. Found {0} in goods line for House-Bill ({1}) / Container({2})";
				Notification.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, String.Format(msg, packs, houseBillXsd.Housebill, packageXsd.ContainerNumber)));
			}
			else
			{
				packageXsd.NumberOfPacks = Convert.ToUInt32(packs);
			}
			packageXsd.PackType = row[PackingColumns.PackageType];
			packageXsd.Volume.Value = row.GetFieldAsZDecimal(PackingColumns.Volume);
			//PackageXsd.Volume.DimensionType = Row[PackingColumns.VolumeUQ];
			packageXsd.Weight.Value = row.GetFieldAsZDecimal(PackingColumns.Weight);
			packageXsd.Weight.DimensionType = row[PackingColumns.WeightUQ];
			packageXsd.GoodsDescription = row[PackingColumns.GoodsDescription];
			houseBillXsd.ShipmentDetails.MarksAndNumbers = row[PackingColumns.MarksAndNumbers];
			PopulatePackingDeclaration(houseBillXsd.Declaration, row);
		}

		void PopulatePackingDeclaration(Xsd.Declaration declarationXsd, FlatFileDataRow packingRow)
		{
			declarationXsd.IsFumigationCert = GetValueFromBooleanField(packingRow, PackingColumns.IsFumigationCert);
			declarationXsd.IsPerishableGoods = GetValueFromBooleanField(packingRow, PackingColumns.IsPerishableGoods);
			declarationXsd.IsPersonalEffects = GetValueFromBooleanField(packingRow, PackingColumns.IsPersonalEffects);
			declarationXsd.IsHazardousGoods = GetValueFromBooleanField(packingRow, PackingColumns.IsHazardousGoods);
			declarationXsd.IsTimber = GetValueFromBooleanField(packingRow, PackingColumns.IsTimber);
			declarationXsd.IsSAC = GetValueFromBooleanField(packingRow, PackingColumns.IsSAC);
			declarationXsd.IsDocuments = GetValueFromBooleanField(packingRow, PackingColumns.IsDocuments) ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
		}

#if DEBUG
		internal
#endif
		bool GetValueFromBooleanField(FlatFileDataRow row, int field)
		{
			ZBool result = false;
			ZBool.TryParse((string)row[field], out result);
			return result;
		}

		Xsd.Shipment FindHouseBill(Xsd.ShipmentCollection houseBills, ZString oceanBill)
		{
			foreach (Xsd.Shipment houseBill in houseBills)
			{
				if (houseBill.ShipmentIdentifier.FindFirst(Enterprise.DataTransfer.Xml.XsdVersion1.ShipmentIdentifierType.Housebill).Value.EqualsIgnoringCase(oceanBill))
				{
					return houseBill;
				}
			}
			return null;
		}

		#endregion

		#region Implementation

		const string OceanBillQualifier = "SEAOBL";
		const string ContainerQualifier = "SEACNT";
		const string HouseBillQualifier = "SEAHBL";
		const string PackingQualifier = "SEAGDS";

		static class OceanBillColumns
		{
			public const int MinimumFieldCount = 31;

			public const int OceanBill = 1;
			public const int MasterHouse = 2;
			public const int MultiOBLUnpack = 3;
			public const int Vessel = 4;
			public const int VesselLloyds = 5;
			public const int Voyage = 6;
			public const int PortOfLoading = 7;
			public const int PortOfDischarge = 8;
			public const int PortOfFirstArrival = 9;
			public const int DateOfArrival = 10;
			public const int ShippingLineCode = 11;
			public const int CarrierCountryCode = 19;
			public const int CarrierCoRegOrCustomsCode = 20;
			public const int PrincipalCountryCode = 29;
			public const int PrincipalCoRegOrCustomsCode = 30;
		}

		static class ContainerColumns
		{
			public const int MinimumFieldCount = 8;

			public const int ContainerNumber = 1;
			public const int SealNumber = 2;
			public const int ContainerType = 3;
			public const int TypeOfContainer = 4;
			public const int ContainerSizeOrISOCode = 5;
			public const int ContainerMode = 6;
			public const int IsShipperOwnedContainer = 7;
		}

		static class HouseBillColumns
		{
			public const int MinimumFieldCount = 37;

			public const int HouseBill = 1;
			public const int MasterHouseBill = 2;
			public const int IsMasterHouse = 3;
			public const int PortOfOrigin = 5;
			public const int PortOfDestination = 6;
			public const int GoodsOrigin = 7;
			public const int PrepaidCollectOther = 8;
			//Consignor
			public const int ConsignorCode = 9;
			public const int ConsignorName = 10;
			public const int ConsignorAddress1 = 11;
			public const int ConsignorAddress2 = 12;
			public const int ConsignorSuburb = 13;
			public const int ConsignorPostCode = 14;
			public const int ConsignorPhone = 15;
			public const int ConsignorFax = 16;
			public const int ConsignorCountryCode = 17;
			public const int ConsignorCoRegOrCustomsCode = 18;
			//Consignee
			public const int ConsigneeCode = 19;
			public const int ConsigneeName = 20;
			public const int ConsigneeAddress1 = 21;
			public const int ConsigneeAddress2 = 22;
			public const int ConsigneeSuburb = 23;
			public const int ConsigneePostCode = 24;
			public const int ConsigneePhone = 25;
			public const int ConsigneeFax = 26;
			public const int ConsigneeCountryCode = 27;
			public const int ConsigneeCoRegOrCustomsCode = 28;
			//Notify
			public const int NotifyCode = 29;
			public const int NotifyName = 30;
			public const int NotifyAddress1 = 31;
			public const int NotifyAddress2 = 32;
			public const int NotifySuburb = 33;
			public const int NotifyPostCode = 34;
			public const int NotifyPhone = 35;
			public const int NotifyFax = 36;
			public const int NotifyCountryCode = 37;
		}

		static class PackingColumns
		{
			public const int MinimumFieldCount = 20;

			public const int HouseBill = 1;
			public const int MasterHouseBill = 2;
			public const int ContainerNumber = 3;
			public const int GoodsDescription = 4;
			public const int MarksAndNumbers = 5;
			public const int PackageType = 6;
			public const int PackageCount = 7;
			public const int Weight = 8;
			public const int WeightUQ = 9;
			public const int Volume = 10;
			public const int VolumeUQ = 11;
			public const int IsFumigationCert = 12;
			public const int IsPerishableGoods = 13;
			public const int Spare1 = 14;
			public const int IsPersonalEffects = 15;
			public const int IsHazardousGoods = 16;
			public const int IsTimber = 17;
			public const int IsDocuments = 18;
			public const int IsSAC = 19;
		}

		#endregion
	}
}
