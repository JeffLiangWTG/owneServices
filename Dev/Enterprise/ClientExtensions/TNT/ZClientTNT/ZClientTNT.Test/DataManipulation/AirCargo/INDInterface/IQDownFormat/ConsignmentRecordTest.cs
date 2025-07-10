using System;
using CargoWise.Types;

namespace Enterprise.Client.TNT.Testing
{
	public class ConsignmentRecordTest : IQDownBaseRecordTest
	{
		public override void TestFieldProperty()
		{
			ConsignmentRecord record = new ConsignmentRecord(DataString);
			AssertEquals("Record Type", "03", record.RecordType);
			AssertEquals("Consignment Number", "940432180 ", record.HouseBill);
			record.HouseBill = "ConsigmentNo";
			AssertEquals("Consignment Number", "ConsigmentNo", record.HouseBill);
			AssertEquals("Consignment Origin", "ADL", record.Origin);
			record.Origin = "SYD";
			AssertEquals("Consignment Origin", "SYD", record.Origin);
			AssertEquals("Consignment Destination", "USO", record.Destination);
			record.Destination = "NZ ";
			AssertEquals("Consignment Destination", "NZ ", record.Destination);
			AssertEquals("Consignor Legacy Code", "20908767", record.ConsignorLegacyCode);
			record.ConsignorLegacyCode = "ConsignorLegacyCode";
			AssertEquals("Consignor Legacy Code", "ConsignorLegacyCode", record.ConsignorLegacyCode);
			AssertEquals("Consignor Company Name", "COVANCE P/L                    ", record.ConsignorName);
			record.ConsignorName = "Consignor Company Name";
			AssertEquals("Consignor Company Name", "Consignor Company Name", record.ConsignorName);
			AssertEquals("Consignor Address 1", "FL 3 4 RESEARCH PARK DR        ", record.ConsignorAddress1);
			record.ConsignorAddress1 = "Consignor Address 1";
			AssertEquals("Consignor Address 1", "Consignor Address 1", record.ConsignorAddress1);
			AssertEquals("Consignor Address 2", "MACQUARIE UNI                  ", record.ConsignorAddress2);
			record.ConsignorAddress2 = "Consignor Address 2";
			AssertEquals("Consignor Address 2", "Consignor Address 2", record.ConsignorAddress2);
			AssertEquals("Consignor City", "NORTH RYDE                     ", record.ConsignorCity);
			record.ConsignorCity = "Consignor City";
			AssertEquals("Consignor City", "Consignor City", record.ConsignorCity);
			AssertEquals("Consignor State", "NEW SOUTH WALES                ", record.ConsignorState);
			record.ConsignorState = "Consignor State";
			AssertEquals("Consignor State", "Consignor State", record.ConsignorState);
			AssertEquals("Consignor Country Code", "AU ", record.ConsignorCountry);
			record.ConsignorCountry = "Consignor Country Code";
			AssertEquals("Consignor Country Code", "Consignor Country Code", record.ConsignorCountry);
			AssertEquals("Consignor Postcode", "2113     ", record.ConsignorPostCode);
			record.ConsignorPostCode = "Consignor Postcode";
			AssertEquals("Consignor Postcode", "Consignor Postcode", record.ConsignorPostCode);
			AssertEquals("Consignor Company Phone", "0288792000  ", record.ConsignorPhone);
			record.ConsignorPhone = "Consignor Company Phone";
			AssertEquals("Consignor Company Phone", "Consignor Company Phone", record.ConsignorPhone);
			AssertEquals("Consignor Contact Phone", "0288792000  ", record.ConsignorContactPhone);
			record.ConsignorContactPhone = "Consignor Contact Phone";
			AssertEquals("Consignor Contact Phone", "Consignor Contact Phone", record.ConsignorContactPhone);
			AssertEquals("Consignor Contact Name", "BOB SMITH             ", record.ConsignorContactName);
			record.ConsignorContactName = "Consignor Contact Name";
			AssertEquals("Consignor Contact Name", "Consignor Contact Name", record.ConsignorContactName);
			AssertEquals("Pickup Company Name", "SLEEP LAB LEVEL 6 MCEWIN BLDG  ", record.PickupName);
			record.PickupName = "Pickup Company Name";
			AssertEquals("Pickup Company Name", "Pickup Company Name", record.PickupName);
			AssertEquals("Pickup Address 1", "ROYAL ADELAIDE HOSPITAL        ", record.PickupAddress1);
			record.PickupAddress1 = "Pickup Address 1";
			AssertEquals("Pickup Address 1", "Pickup Address 1", record.PickupAddress1);
			AssertEquals("Pickup Address 2", "NORTH TERRACE                  ", record.PickupAddress2);
			record.PickupAddress2 = "Pickup Address 2";
			AssertEquals("Pickup Address 2", "Pickup Address 2", record.PickupAddress2);
			AssertEquals("Pickup City", "ADELAIDE                       ", record.PickupCity);
			record.PickupCity = "Pickup City";
			AssertEquals("Pickup City", "Pickup City", record.PickupCity);
			AssertEquals("Pickup State", "SOUTH AUSTRALIA                ", record.PickupState);
			record.PickupState = "Pickup State";
			AssertEquals("Pickup State", "Pickup State", record.PickupState);
			AssertEquals("Pickup Country Code", "AU ", record.PickupCountry);
			record.PickupCountry = "Pickup Country Code";
			AssertEquals("Pickup Country Code", "Pickup Country Code", record.PickupCountry);
			AssertEquals("Pickup Postcode", "5000     ", record.PickupPostCode);
			record.PickupPostCode = "Pickup Postcode";
			AssertEquals("Pickup Postcode", "Pickup Postcode", record.PickupPostCode);
			AssertEquals("Pickup Company Phone", "08485648144 ", record.PickupPhone);
			record.PickupPhone = "Pickup Company Phone";
			AssertEquals("Pickup Company Phone", "Pickup Company Phone", record.PickupPhone);
			AssertEquals("Pickup Contact Phone", "08468451155 ", record.PickupContactPhone);
			record.PickupContactPhone = "Pickup Contact Phone";
			AssertEquals("Pickup Contact Phone", "Pickup Contact Phone", record.PickupContactPhone);
			AssertEquals("Pickup Contact Name", "GAIL KOSHOREK         ", record.PickupContactName);
			record.PickupContactName = "Pickup Contact Name";
			AssertEquals("Pickup Contact Name", "Pickup Contact Name", record.PickupContactName);
			AssertEquals("Consignee Company Name", "HENRY FORD HOSPITAL            ", record.ConsigneeName);
			record.ConsigneeName = "Consignee Company Name";
			AssertEquals("Consignee Company Name", "Consignee Company Name", record.ConsigneeName);
			AssertEquals("Consignee Address 1", "SLEEP DISORDERS RESEARCH CENT  ", record.ConsigneeAddress1);
			record.ConsigneeAddress1 = "Consignee Address 1";
			AssertEquals("Consignee Address 1", "Consignee Address 1", record.ConsigneeAddress1);
			AssertEquals("Consignee Address 2", "2799 W GRAND BLVD CFP 3NIK     ", record.ConsigneeAddress2);
			record.ConsigneeAddress2 = "Consignee Address 2";
			AssertEquals("Consignee Address 2", "Consignee Address 2", record.ConsigneeAddress2);
			AssertEquals("Consignee City", "DETROIT                        ", record.ConsigneeCity);
			record.ConsigneeCity = "Consignee City";
			AssertEquals("Consignee City", "Consignee City", record.ConsigneeCity);
			AssertEquals("Consignee State", "MI                             ", record.ConsigneeState);
			record.ConsigneeState = "Consignee State";
			AssertEquals("Consignee State", "Consignee State", record.ConsigneeState);
			AssertEquals("Consignee Country Code", "US ", record.ConsigneeCountry);
			record.ConsigneeCountry = "Consignee Country Code";
			AssertEquals("Consignee Country Code", "Consignee Country Code", record.ConsigneeCountry);
			AssertEquals("Consignee Postcode", "48202    ", record.ConsigneePostCode);
			record.ConsigneePostCode = "Consignee Postcode";
			AssertEquals("Consignee Postcode", "Consignee Postcode", record.ConsigneePostCode);
			AssertEquals("Consignee Company Phone", "68454578982 ", record.ConsigneePhone);
			record.ConsigneePhone = "Consignee Company Phone";
			AssertEquals("Consignee Company Phone", "Consignee Company Phone", record.ConsigneePhone);
			AssertEquals("Consignee Contact Phone", "12345678901 ", record.ConsigneeContactPhone);
			record.ConsigneeContactPhone = "Consignee Contact Phone";
			AssertEquals("Consignee Contact Phone", "Consignee Contact Phone", record.ConsigneeContactPhone);
			AssertEquals("Consignee Contact Name", "GAIL KOSHOREK         ", record.ConsigneeContactName);
			record.ConsigneeContactName = "Consignee Contact Name";
			AssertEquals("Consignee Contact Name", "Consignee Contact Name", record.ConsigneeContactName);
			AssertEquals("Delivery Company Name", "DELIVERY COMPANY NAME          ", record.DeliveryName);
			record.DeliveryName = "Delivery Company Name";
			AssertEquals("Delivery Company Name", "Delivery Company Name", record.DeliveryName);
			AssertEquals("Delivery Address 1", "DELIVERY ADDRESS 1             ", record.DeliveryAddress1);
			record.DeliveryAddress1 = "Delivery Address 1";
			AssertEquals("Delivery Address 1", "Delivery Address 1", record.DeliveryAddress1);
			AssertEquals("Delivery Address 1", "DELIVERY ADDRESS 2             ", record.DeliveryAddress2);
			record.DeliveryAddress2 = "Delivery Address 1";
			AssertEquals("Delivery Address 1", "Delivery Address 1", record.DeliveryAddress2);
			AssertEquals("Delivery City", "DELIVERY CITY                  ", record.DeliveryCity);
			record.DeliveryCity = "Delivery City";
			AssertEquals("Delivery City", "Delivery City", record.DeliveryCity);
			AssertEquals("Delivery State", "DELIVERY STATE                 ", record.DeliveryState);
			record.DeliveryState = "Delivery State";
			AssertEquals("Delivery State", "Delivery State", record.DeliveryState);
			AssertEquals("Delivery Country Code", "CAN", record.DeliveryCountry);
			record.DeliveryCountry = "Delivery Country Code";
			AssertEquals("Delivery Country Code", "Delivery Country Code", record.DeliveryCountry);
			AssertEquals("Delivery Postcode", "123456789", record.DeliveryPostCode);
			record.DeliveryPostCode = "Delivery Postcode";
			AssertEquals("Delivery Postcode", "Delivery Postcode", record.DeliveryPostCode);
			AssertEquals("Delivery Company Phone", "012345678901", record.DeliveryPhone);
			record.DeliveryPhone = "Delivery Company Phone";
			AssertEquals("Delivery Company Phone", "Delivery Company Phone", record.DeliveryPhone);
			AssertEquals("Delivery Contact Phone", "234567890123", record.DeliveryContactPhone);
			record.DeliveryContactPhone = "Delivery Contact Phone";
			AssertEquals("Delivery Contact Phone", "Delivery Contact Phone", record.DeliveryContactPhone);
			AssertEquals("Delivery Contact Name", "DELIVERY CONTACT NAME ", record.DeliveryContactName);
			record.DeliveryContactName = "Delivery Contact Name";
			AssertEquals("Delivery Contact Name", "Delivery Contact Name", record.DeliveryContactName);
			AssertEquals("Document Indicator", "N", record.DocumentIndicator);
			record.DocumentIndicator = "Document Indicator";
			AssertEquals("Document Indicator", "Document Indicator", record.DocumentIndicator);
			AssertEquals("Terms Of Payment", "S", record.TermsOfPayment);
			record.TermsOfPayment = "Terms Of Payment";
			AssertEquals("Terms Of Payment", "Terms Of Payment", record.TermsOfPayment);
			AssertEquals("Value Of Goods", 1233233234.34m, record.GoodsValue);
			AssertEquals("Value Of Goods", "1233233234.34", record[ConsignmentRecord.Schema.GoodsValue.Name]);
			record.GoodsValue = 4234.233m;
			AssertEquals("Value Of Goods", 4234.23m, record.GoodsValue);
			AssertEquals("Value Of Goods", "4234.23", record[ConsignmentRecord.Schema.GoodsValue.Name]);
			record[ConsignmentRecord.Schema.GoodsValue.Name] = "1232.3211";
			AssertEquals("Value Of Goods", 1232.32m, record.GoodsValue);
			AssertEquals("Value Of Goods", "1232.3211", record[ConsignmentRecord.Schema.GoodsValue.Name]);
			AssertEquals("GoodsCurrency", "AUD", record.GoodsCurrency);
			record.GoodsCurrency = "Currency";
			AssertEquals("GoodsCurrency", "Currency", record.GoodsCurrency);
			AssertEquals("Total Number of Pieces", (short)12345, record.PackageCount);
			AssertEquals("Total Number of Pieces", "12345 ", record[ConsignmentRecord.Schema.PackageCount.Name]);
			record.PackageCount = 32;
			AssertEquals("Total Number of Pieces", (short)32, record.PackageCount);
			AssertEquals("Total Number of Pieces", "32", record[ConsignmentRecord.Schema.PackageCount.Name]);
			record[ConsignmentRecord.Schema.PackageCount.Name] = " 4567 ";
			AssertEquals("Total Number of Pieces", (short)4567, record.PackageCount);
			AssertEquals("Total Number of Pieces", " 4567 ", record[ConsignmentRecord.Schema.PackageCount.Name]);
			AssertEquals("Total Gross Weight", 123456.62m, record.Weight);
			AssertEquals("Total Gross Weight", "123456.620", record[ConsignmentRecord.Schema.Weight.Name]);
			record.Weight = 3423.433m;
			AssertEquals("Total Gross Weight", 3423.44m, record.Weight);
			AssertEquals("Total Gross Weight", "3423.44", record[ConsignmentRecord.Schema.Weight.Name]);
			record[ConsignmentRecord.Schema.Weight.Name] = "1234.23401";
			AssertEquals("Total Gross Weight", 1234.24m, record.Weight);
			AssertEquals("Total Gross Weight", "1234.23401", record[ConsignmentRecord.Schema.Weight.Name]);
			AssertEquals("T-Doc Number", "T-DOC1234", record.TDoc);
			record.TDoc = "T-Doc Number";
			AssertEquals("T-Doc Number", "T-Doc Number", record.TDoc);
			AssertEquals("Delivery Instructions", "Delivery Instructions for the remark    ", record.Remarks);
			record.Remarks = "Delivery Instructions";
			AssertEquals("Delivery Instructions", "Delivery Instructions", record.Remarks);
			AssertEquals("Consignee Company Telex", "123456789012", record.ConsigneeTelex);
			record.ConsigneeTelex = "Consignee Company Telex";
			AssertEquals("Consignee Company Telex", "Consignee Company Telex", record.ConsigneeTelex);
			AssertEquals("Consignee Company Fax", "12345678901", record.ConsigneeFax);
			record.ConsigneeFax = "Consignee Company Fax";
			AssertEquals("Consignee Company Fax", "Consignee Company Fax", record.ConsigneeFax);
			AssertEquals("Spaces", ZString.Replicate(' ', 15), record.spaces);
			AssertEquals("Record Delimiter", ".", record.RecordDelimiter);
		}

#region TestHumanReadable
		public override void TestHumanReadable()
		{
			ConsignmentRecord record = new ConsignmentRecord(DataString);
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, record.HouseBill), record.HumanReadable);
			record.HouseBill = "BOB";
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, "BOB"), record.HumanReadable);
		}

		ZString GenerateExpectedHumanReadable(ZString recordType, ZString houseBill)
		{
			return ZString.Format("Record {0} (Con Number={1})", recordType, houseBill);
		}

#endregion
		protected override IQDownBaseRecord GetRecord(ZString rawData)
		{
			return new ConsignmentRecord(rawData);
		}

		protected override ZString DataString
		{
			get
			{
				return "03940432180 ADLUSO20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS1233233234.34AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
			}
		}

		protected override Type ExpectedRecordType
		{
			get
			{
				return typeof(ConsignmentRecord);
			}
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 57;
			}
		}
	}
}
