using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(IQDownAirCargoCollection))]
	public class IQDownAirCargoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IQDownAirCargoCollection>
	{
		public void TestConstructor()
		{
			IQDownAirCargoCollection collection = new IQDownAirCargoCollection(Factory);
			AssertNotNull("Collection should not be null", collection);
			AssertEquals("AllowNew should be false", false, collection.AllowNew);
		}

		public void TestIndexer_UsingInteger()
		{
			IQDownAirCargoCollection collection = new IQDownAirCargoCollection(Factory);
			IQDownAirCargo airCargo = new IQDownAirCargo(Factory, new FlightRecord(FlightDetailLine), "SYD");
			collection.Add(airCargo);
			IQDownAirCargo chkRecord = collection[0];
			AssertNotNull("ChkRecord", chkRecord);
			AssertEquals("Record should be the same", airCargo, chkRecord);
		}

		public void TestIndexer_UsingFlightRecord()
		{
			IQDownAirCargoCollection collection = new IQDownAirCargoCollection(Factory);
			FlightRecord record = new FlightRecord(FlightDetailLine);
			IQDownAirCargo airCargo1 = new IQDownAirCargo(Factory, record, "SYD");
			collection.Add(airCargo1);
			record = new FlightRecord(FlightDetailLine);
			record.MasterBill = "MAWB2";
			IQDownAirCargo airCargo2 = new IQDownAirCargo(Factory, record, "SYD");
			collection.Add(airCargo2);
			record = new FlightRecord(FlightDetailLine);
			record.MasterBill = "MAWB3";
			IQDownAirCargo airCargo3 = new IQDownAirCargo(Factory, record, "SYD");
			collection.Add(airCargo3);
			AssertEquals("PreCondition: Collection has 3 elements", 3, collection.Count);
			record = new FlightRecord(FlightDetailLine);
			record.MasterBill = "MAWB3";
			IQDownAirCargo chkAirCargo = collection[record];
			AssertNotNull("ChkAirCargo", chkAirCargo);
			AssertEquals("ChkAirCargo should be the same AirCargo3", airCargo3.GetHashCode(), chkAirCargo.GetHashCode());
			record.MasterBill = airCargo2.MasterBill;
			chkAirCargo = collection[record];
			AssertNotNull("ChkAirCargo", chkAirCargo);
			AssertEquals("ChkAirCargo should be the same AirCargo2", airCargo2.GetHashCode(), chkAirCargo.GetHashCode());
			record.MasterBill = airCargo1.MasterBill;
			chkAirCargo = collection[record];
			AssertNotNull("ChkAirCargo", chkAirCargo);
			AssertEquals("ChkAirCargo should be the same AirCargo1", airCargo1.GetHashCode(), chkAirCargo.GetHashCode());
		}

		#region Implementation
		protected override IQDownAirCargoCollection GetCollectionToTest()
		{
			return new IQDownAirCargoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IQDownAirCargo(Factory, new FlightRecord(FlightDetailLine), "SYD");
		}

		protected new IQDownAirCargoCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}

		const string FlightDetailLine = "01BA0151SINSYD150705A12527013486 M0000151  SINSYDTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		#endregion
	}
}
