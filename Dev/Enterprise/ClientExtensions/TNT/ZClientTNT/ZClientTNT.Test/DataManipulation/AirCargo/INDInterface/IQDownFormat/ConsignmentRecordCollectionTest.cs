using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	public class ConsignmentRecordCollectionTest : TestCase
	{
		public void TestConstructor()
		{
			ConsignmentRecordCollection collection = new ConsignmentRecordCollection();
			AssertEquals(0, collection.Count);
		}

		public void TestAdd()
		{
			ConsignmentRecordCollection collection = new ConsignmentRecordCollection();
			ConsignmentRecord record = new ConsignmentRecord(Line1);
			collection.Add(record);
			AssertEquals(1, collection.Count);
			ConsignmentRecord chkRecord = collection[0];
			AssertEquals(record, chkRecord);
			record = new ConsignmentRecord(Line2);
			collection.Add(record);
			AssertEquals(2, collection.Count);
			ConsignmentRecord record3 = new ConsignmentRecord(Line3);
			AssertEquals("PreCondition: Record3 Value", 2500.60m, record3.GoodsValue);
			AssertEquals("PreCondition: Record3 PackageCount", (short)60, record3.PackageCount);
			AssertEquals("PreCondition: Record3 Weight", 300.78m, record3.Weight);
			record = collection[record3.HouseBill];
			AssertNotNull("PreCondition: Record with Consignment Number '" + record3.HouseBill + "' should exist in collection", record);
			AssertEquals("PreCondition: Record should have the same Consignment Number as Record3", record3.HouseBill, record.HouseBill);
			AssertEquals("PreCondition: Record Value", 3500.40m, record.GoodsValue);
			AssertEquals("PreCondition: Record PackageCount", (short)40, record.PackageCount);
			AssertEquals("PreCondition: Record Weight", 200.22m, record.Weight);
			collection.Add(record3);
			AssertEquals("No new record added. Exisiting should have been merged", 2, collection.Count);
			record = collection[record3.HouseBill];
			AssertNotNull("Record with Consignment Number '" + record3.HouseBill + "' should exist in collection", record);
			AssertEquals("Record should have the same Consignment Number as Record3", record3.HouseBill, record.HouseBill);
			AssertEquals("Record Value", 6001.00m, record.GoodsValue);
			AssertEquals("Record PackageCount", (short)100, record.PackageCount);
			AssertEquals("Record Weight", 501.00m, record.Weight);
		}

		public void TestIndexer_UsingInteger()
		{
			ConsignmentRecordCollection collection = new ConsignmentRecordCollection();
			ConsignmentRecord record = new ConsignmentRecord(Line1);
			collection.Add(record);
			AssertEquals(1, collection.Count);
			object chkRecord = collection[0];
			AssertNotNull(chkRecord);
			AssertEquals(record, chkRecord);
		}

		public void TestIndexer_UsingHouseBill()
		{
			ConsignmentRecordCollection collection = new ConsignmentRecordCollection();
			ConsignmentRecord record1 = new ConsignmentRecord(Line1);
			collection.Add(record1);
			AssertEquals(1, collection.Count);
			ConsignmentRecord record2 = new ConsignmentRecord(Line2);
			collection.Add(record2);
			AssertEquals(2, collection.Count);
			ConsignmentRecord chkRecord = collection[record2.HouseBill];
			AssertNotNull("Record with Consignment Number '" + record2.HouseBill + "' should exist in collection", chkRecord);
			AssertEquals("Collection should have return Record2", record2, chkRecord);
			chkRecord = collection[record1.HouseBill];
			AssertNotNull("Record with Consignment Number '" + record1.HouseBill + "' should exist in collection", chkRecord);
			AssertEquals("Collection should have return Record1", record1, chkRecord);
		}

		#region Implementation
		const string Line1 = "03940432180 ADLUSO20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS1233233234.34AUD123456123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string Line2 = "03345544540 SINSYD20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS      3500.40AUD    40   200.212T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string Line3 = "03345544540 SINSYD20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS      2500.60AUD    60   300.778T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		#endregion
	}
}
