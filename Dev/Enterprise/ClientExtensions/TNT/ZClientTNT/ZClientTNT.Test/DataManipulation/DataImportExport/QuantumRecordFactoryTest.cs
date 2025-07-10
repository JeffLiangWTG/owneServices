using CargoWise.Types;
using Enterprise.Client.TNT.NZ;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	public class QuantumRecordFactoryTest : TestCase
	{
		public void TestNewRecord()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			QuantumRecordFactory factory = new QuantumRecordFactory();
			IQDownBaseRecord record = factory.NewRecord("SYD", FlightLine);
			AssertNotNull("Record should be not null", record);
			AssertEquals("Record type should be QuantumConsolRecord", typeof(QuantumConsolRecord), record.GetType());
			AssertEquals("MBagNo should be empty", ZString.Empty, factory.TestMBagNo);
			record = factory.NewRecord("SYD", TBagLine);
			AssertNull("Record should be not null", record);
			AssertEquals("MBagNo should be not empty", "B1927029  ", factory.TestMBagNo);
			record = factory.NewRecord("SYD", ConsignmentLine);
			AssertNotNull("Record should be not null", record);
			AssertEquals("Record type should be QuantumShipmentRecord", typeof(QuantumShipmentRecord), record.GetType());
			AssertEquals("MBagNo should be not empty", "B1927029  ", factory.TestMBagNo);
			QuantumShipmentRecord shipmentRecord = (QuantumShipmentRecord)record;
			AssertEquals("ShipmentRecord.MBagNo should be not empty", "B1927029  ", shipmentRecord.MBagNo);
			AssertEquals("ShipmentRecord.BranchCode should be not empty", "SYD", shipmentRecord.BranchCode);
			record = factory.NewRecord("SYD", ConsignmentNoteLine);
			AssertNotNull("Record should be not null", record);
			AssertEquals("Record type should be QuantumShipmentNotesRecord", typeof(QuantumShipmentNotesRecord), record.GetType());
			AssertEquals("MBagNo should be not empty", "B1927029  ", factory.TestMBagNo);
			record = factory.NewRecord("SYD", "01");
			AssertNull("Record should be null", record);
			AssertEquals("MBagNo should be not empty", "B1927029  ", factory.TestMBagNo);
			record = factory.NewRecord("SYD", "BLAH BLAH");
			AssertNull("Record should be null", record);
			AssertEquals("MBagNo should be not empty", "B1927029  ", factory.TestMBagNo);
		}

		public void TestNZQuantumShipmentRecord()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			QuantumRecordFactory factory = new QuantumRecordFactory();
			QuantumShipmentRecord shipmentRecord = (QuantumShipmentRecord)factory.NewRecord("AKL", ConsignmentLine);
			AssertEquals("Record type should be a NZQuantumShipmentRecord", typeof(NZQuantumShipmentRecord), shipmentRecord.GetType());
		}

		const string FlightLine = "01BA0151SINSYD150705A12527013486 M0000151  SINSYDTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		const string TBagLine = "02B1927029  TBAGNO    SINSYD                                                                                                                                                                                                                                                                                                                                                                                                                                                                             .";
		const string ConsignmentLine = "03940432180 ADLUSO20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS1233233234.34AUD123456123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ConsignmentNoteLine = "04940432180 01123456789012345DOCUMENTS AND DOCS.IN FOLDER                                                  " + "DEscription 2                                                                 " + "Description 3                                                                 " + "MELIAHEX2456789012                                                                                                                                                                                                                .";
	}
}
