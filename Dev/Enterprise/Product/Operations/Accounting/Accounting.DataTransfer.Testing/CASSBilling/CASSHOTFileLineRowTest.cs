using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public abstract class CASSHOTFileLineRowTest : CASSHOTFileDataRowTest
	{
		[TestDate(2011, 07, 13)]
		public override void TestPublicFields()
		{
			base.TestPublicFields();

			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.AirlinePrefix, "AIR");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.AWBSerialNumber, "111");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.AgentCode, "CODE");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.DateAWBExecution, "081123");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.Origin, "SYD");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.Destination, "DEN");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.Weight, "120");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.WeightIndicator, "K");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.CurrencyCode, "USD");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.DateOfArrival, "110612");
			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.DateOfDelivery, "110625");

			AssertEquals("AIR", LineRowForTest.AirlinePrefix);
			AssertEquals("111", LineRowForTest.AWBSerialNumber);
			AssertEquals("CODE", LineRowForTest.AgentCode);
			AssertEquals(new ZDateTime(2008, 11, 23), LineRowForTest.DateAWBExecution);
			AssertEquals("SYD", LineRowForTest.Origin);
			AssertEquals("DEN", LineRowForTest.Destination);
			AssertEquals(12.0M, LineRowForTest.Weight);
			AssertEquals("KG", LineRowForTest.WeightUnit);
			AssertEquals("USD", LineRowForTest.Currency);
			AssertEquals("", LineRowForTest.VATIndicator);

			AssertEquals(new ZDateTime(2011, 06, 12), LineRowForTest.DateOfArrival);
			AssertEquals(new ZDateTime(2011, 06, 25), LineRowForTest.DateOfDelivery);

			LineRowForTest.SetField(CASSHOTFileLineRow.Schema.WeightIndicator, "L");
			AssertEquals("LB", LineRowForTest.WeightUnit);
		}

		protected CASSHOTFileLineRow LineRowForTest
		{
			get { return (CASSHOTFileLineRow)DataRowForTest; }
		}
	}
}
