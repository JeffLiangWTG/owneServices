using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoFlatFileConverter : FlatFileConverter
	{
		public AirCargoFlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection rows)
		{
			if (IsDataValid(rows))
			{
				Xsd.Consol masterBill = valueObject as Xsd.Consol;
				PopulateMasterBill(masterBill, rows[0]);

				for (int i = 1; i < rows.Count - 1; i++)
				{
					FlatFileDataRow hBLRow = rows[i];
					FlatFileDataRow goodsRow = rows[++i];

					if (ValidateHouseBillData(hBLRow, goodsRow, i, Notification))
					{
						Xsd.Shipment houseBill = masterBill.Shipments.AddNew();
						try
						{
							PopulateHouseBill(houseBill, hBLRow, goodsRow);
						}
						catch (FormatException exception)
						{
							Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, String.Format("{0} [Line {1}]", exception.Message, i)));
						}
					}
				}
			}
			else
			{
				string corruptDataField = rows.Count > 0 && rows[0].FieldCount > 0 ? rows[0].GetField(0).ToString() : "no data";
				Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, String.Format("Corrupt or invalid Air Cargo CSV file. Expected {0} but found {1} at line 1.", MasterBillQualifier, corruptDataField)));
			}
		}

		#region ValidateHouseBillPackingData

		bool ValidateHouseBillData(FlatFileDataRow hBLRow, FlatFileDataRow goodsRow, int lineNumber, INotifications notifier)
		{
			bool result = true;
			if (hBLRow.FieldCount < HouseBillColumns.MinimumFieldCount)
			{
				notifier.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, String.Format("Expected at least {0} fields in line {1} but found only {2}.", HouseBillColumns.MinimumFieldCount, lineNumber, hBLRow.FieldCount)));
				result = false;
			}

			if (goodsRow.FieldCount < PackingColumns.MinimumFieldCount)
			{
				notifier.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, String.Format("Expected at least {0} fields in line {1} but found only {2}.", PackingColumns.MinimumFieldCount, lineNumber + 1, goodsRow.FieldCount)));
				result = false;
			}

			ZString qualifierFormatErrorInfo = "";

			if (hBLRow[0] != HouseBillQualifier)
			{
				qualifierFormatErrorInfo += ZString.Format(" Expected {0} but found {1} at line {2}.", HouseBillQualifier, hBLRow[0], lineNumber);
			}

			if (goodsRow[0] != PackingQualifier)
			{
				qualifierFormatErrorInfo += ZString.Format(" Expected {0} but found {1} at line {2}.", PackingQualifier, goodsRow[0], lineNumber + 1);
			}

			if (qualifierFormatErrorInfo != "")
			{
				notifier.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, String.Format("Unknown qualifier detected.{0}", qualifierFormatErrorInfo)));
				result = false;
			}
			return result;
		}
		#endregion

		#region MasterBill

		void PopulateMasterBill(Xsd.Consol masterBill, FlatFileDataRow row)
		{
			Xsd.ConsolIdentifier id = masterBill.ConsolIdentifier.AddNew();
			id.Value = row[MasterBillColumns.MasterBill];
			id.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;

			if (!row[MasterBillColumns.MasterHouseBill].IsEmpty)
			{
				Xsd.ConsolIdentifier masterHouse = masterBill.ConsolIdentifier.AddNew();
				masterHouse.Value = row[MasterBillColumns.MasterHouseBill];
				masterHouse.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			}

			masterBill.ConsolDetail.PortOfLoading.Port.Value = row[MasterBillColumns.PortOfLoading];
			masterBill.ConsolDetail.PortOfDischarge.Port.Value = row[MasterBillColumns.PortOfDischarge];
			masterBill.ConsolDetail.PortFirstArrival.Port.Value = row[MasterBillColumns.PortOfFirstArrival];

			Xsd.FlightWithFlightNumber voyageFlight = new Xsd.FlightWithFlightNumber();

			ZDateTime arrivalDate;
			if (ZDateTime.TryParseExact(row[MasterBillColumns.DateofArrival], out arrivalDate, "yyyyMMdd"))
			{
				voyageFlight.ETA = arrivalDate;
			}
			voyageFlight.FlightNoJourneyNoTruckRegNo = row[MasterBillColumns.Voyage];
			masterBill.ConsolDetail.Item = voyageFlight;
		}

		bool IsDataValid(FlatFileDataRowCollection rows)
		{
			return rows.Count > 0 && rows[0].FieldCount > 0 && (rows[0].GetField(0) == MasterBillQualifier);
		}

		#endregion

		#region HouseBill

		void PopulateHouseBill(Xsd.Shipment houseBillXsd, FlatFileDataRow row, FlatFileDataRow packingRow)
		{
			Xsd.ShipmentIdentifier id = houseBillXsd.ShipmentIdentifier.AddNew();
			id.Value = row[HouseBillColumns.HouseBill];
			id.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			Xsd.ShipmentIdentifier masterHouseId = houseBillXsd.ShipmentIdentifier.AddNew();
			masterHouseId.Value = row[HouseBillColumns.MasterHouseBill];
			masterHouseId.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.CoLoadMaster;

			houseBillXsd.ShipmentDetails.PortofDestination.Port.Value = row[HouseBillColumns.PortOfDestination];
			houseBillXsd.ShipmentDetails.PortOfOrigin.Port.Value = row[HouseBillColumns.PortOfOrigin];

			PopulateConsignor(houseBillXsd, row);
			PopulateConsignee(houseBillXsd, row);

			PopulateDeclaration(houseBillXsd.Declaration, row, packingRow);

			PopulatePacking(houseBillXsd, row, packingRow);
		}

		void PopulatePacking(Xsd.Shipment houseBillXsd, FlatFileDataRow row, FlatFileDataRow packingRow)
		{
			houseBillXsd.ShipmentDetails.TotalOuterPacksQty.Value = packingRow.GetFieldAsZDecimal(PackingColumns.PackageCount);
			houseBillXsd.ShipmentDetails.Weight.Value = packingRow.GetFieldAsZDecimal(PackingColumns.Weight);
			houseBillXsd.ShipmentDetails.Weight.DimensionType = packingRow[PackingColumns.WeightUQ];
			houseBillXsd.ShipmentDetails.GoodsDescription = packingRow[PackingColumns.GoodsDescription];

			if (row.FieldCount > HouseBillColumns.GoodsValue)
			{
				houseBillXsd.ShipmentDetails.GoodsValue = new Xsd.FinancialValue();

				try
				{
					houseBillXsd.ShipmentDetails.GoodsValue.Value = ZDecimal.Parse(row[HouseBillColumns.GoodsValue]);
				}
				catch (FormatException exception)
				{
					throw new FormatException(string.Format("Corrupt or invalid Air Cargo CSV file. Could not parse Goods Value [{0}].", row[HouseBillColumns.GoodsValue]), exception);
				}

				houseBillXsd.ShipmentDetails.GoodsValue.CurrencyCode = row[HouseBillColumns.GoodsCurrency];
			}
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
					if (!row[HouseBillColumns.ConsigneeCoRegOrCustomsCode].IsEmpty)
					{
						Xsd.RegistrationNumber aBN = houseBillXsd.ShipmentDetails.Consignee.OrganisationDetails.RegistrationNumbers.AddNew();
						aBN.NumberType = Xsd.RegistrationNumberTypes.GST;
						aBN.CountryOfRegistration = country.RN_Code;
						aBN.Number = row[HouseBillColumns.ConsigneeCoRegOrCustomsCode];
					}
				}
			}
		}

		void PopulateDeclaration(Xsd.Declaration declarationXsd, FlatFileDataRow row, FlatFileDataRow packingRow)
		{
			declarationXsd.PaymentTerms = row[HouseBillColumns.PrepaidCollectOther];
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

		#endregion
		#region Packing

		#endregion

		#region Implementation

		const string MasterBillQualifier = "AIRMWB";
		const string HouseBillQualifier = "AIRHWB";
		const string PackingQualifier = "AIRGDS";

		static class MasterBillColumns
		{
			public const int MinimumFieldCount = 21;

			public const int MasterBill = 1;
			public const int MasterHouseBill = 2;
			public const int Voyage = 5;
			public const int PortOfLoading = 7;
			public const int PortOfDischarge = 8;
			public const int PortOfFirstArrival = 9;
			public const int DateofArrival = 10;
			public const int ShippingLineCode = 11;
			public const int CarrierCountryCode = 19;
			public const int CarrierCoRegOrCustomsCode = 20;
		}

		static class HouseBillColumns
		{
			public const int MinimumFieldCount = 42;

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

			//Additional Columns
			public const int GoodsValue = 42;
			public const int GoodsCurrency = 43;
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
