using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(XXXAirCargoCollection))]
	public class XXXAirCargoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<XXXAirCargoCollection>
	{
		public void TestConstructor()
		{
			XXXAirCargoCollection collection = new XXXAirCargoCollection(Factory);
			AssertNotNull("Collection should not be null", collection);
			AssertEquals("AllowNew should be false", false, collection.AllowNew);
		}

		public override void TestAddNew()
		{
			Assert(true);
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		public void TestIndexer_UsingInteger()
		{
			XXXAirCargoCollection collection = new XXXAirCargoCollection(Factory);
			XXXAirCargo airCargo = new XXXAirCargo(Factory, FlightRec, new ConsignmentRecord(ConsignmentLine), "SYD");
			collection.Add(airCargo);
			XXXAirCargo chkRecord = collection[0];
			AssertNotNull("ChkRecord", chkRecord);
			AssertEquals("Record should be the same", airCargo, chkRecord);
		}

		public void TestIndexer_UsingConsignmentRecord()
		{
			XXXAirCargoCollection collection = new XXXAirCargoCollection(Factory);
			ConsignmentRecord hawb1 = new ConsignmentRecord(ConsignmentLine);
			hawb1.HouseBill = "HAWB1";
			hawb1.Origin = "NZAKL";
			hawb1.Destination = "AUSYD";
			XXXAirCargo airCargo1 = new XXXAirCargo(Factory, FlightRec, hawb1, "SYD");
			collection.Add(airCargo1);
			ConsignmentRecord hawb2 = new ConsignmentRecord(ConsignmentLine);
			hawb2.HouseBill = "HAWB1";
			hawb2.Origin = "NZAKL";
			hawb2.Destination = "AUMEL";
			XXXAirCargo airCargo2 = new XXXAirCargo(Factory, FlightRec, hawb2, "SYD");
			collection.Add(airCargo2);
			ConsignmentRecord hawb3 = new ConsignmentRecord(ConsignmentLine);
			hawb3.HouseBill = "HAWB3";
			hawb3.Origin = "NZAKL";
			hawb3.Destination = "AUMEL";
			XXXAirCargo airCargo3 = new XXXAirCargo(Factory, FlightRec, hawb3, "SYD");
			collection.Add(airCargo3);
			AssertEquals("PreCondition: Collection has 3 elements", 3, collection.Count);
			ConsignmentRecord chkRecord = new ConsignmentRecord(ConsignmentLine);
			chkRecord.HouseBill = "HAWB1";
			chkRecord.Origin = "NZAKL";
			chkRecord.Destination = "AUSYD";
			XXXAirCargo chkAirCargo = collection[chkRecord];
			AssertNotNull("ChkAirCargo", chkAirCargo);
			AssertEquals("ChkAirCargo should be the same AirCargo1", airCargo1, chkAirCargo);
			chkRecord.Destination = "AUMEL";
			chkAirCargo = collection[chkRecord];
			AssertNotNull("ChkAirCargo", chkAirCargo);
			AssertEquals("ChkAirCargo should be the same AirCargo2", airCargo2, chkAirCargo);
			chkRecord.Origin = "USLAX";
			chkAirCargo = collection[chkRecord];
			AssertNull("ChkAirCargo is not found", chkAirCargo);
			chkRecord.HouseBill = "HAWB3";
			chkRecord.Origin = "NZAKL";
			chkRecord.Destination = "AUMEL";
			chkAirCargo = collection[chkRecord];
			AssertNotNull("ChkAirCargo", chkAirCargo);
			AssertEquals("ChkAirCargo should be the same AirCargo3", airCargo3, chkAirCargo);
		}

		#region Implementation
		#region FlightRec
		FlightRecord FlightRec
		{
			get
			{
				if (fFlightRec == null)
				{
					fFlightRec = new FlightRecord(FlightDetailLine);
				}

				return fFlightRec;
			}
		}

		FlightRecord fFlightRec;
		#endregion
		protected override XXXAirCargoCollection GetCollectionToTest()
		{
			return new XXXAirCargoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new XXXAirCargo(Factory, FlightRec, new ConsignmentRecord(ConsignmentLine), "SYD");
		}

		protected new XXXAirCargoCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}

		const string FlightDetailLine = "01BA0151SINWHR150705A12527013486 M0000151  SINWHRTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		const string ConsignmentLine = "03940432180 SINWHR20908767HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             SG 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     WHERE IS THIS STATE            AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SA                             AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "NS1233233234.34AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		#endregion
	}
}
