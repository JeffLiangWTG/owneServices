using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class NavisionFlatFileDataRowTest : TestCase
	{
		public void TestSetField()
		{
			NavisionFlatFileDataRow row = new NavisionFlatFileDataRow(5);
			FlatFileFieldProperty field1 = new FlatFileFieldProperty(0, 10);
			FlatFileFieldProperty field2 = new FlatFileFieldProperty(1, 8);
			FlatFileFieldProperty field3 = new FlatFileFieldProperty(2, 20);
			FlatFileFieldProperty field4 = new FlatFileFieldProperty(3, 2);
			FlatFileFieldProperty field5 = new FlatFileFieldProperty(4, 20);
			ZString stringValue = "5 L\t?>*ev\ve\"l 2,\r Co\f'te\n Ro?ad.!";
			row.SetField(field1, 123456.7890m);
			row.SetField(field2, new ZDateTime(2006, 2, 8, 17, 12, 42));
			row.SetField(field3, stringValue);
			row.SetField(field4, (ZInt)69);
			row.SetField(field5, stringValue, true);
			AssertEquals("Field 1", 123456.79m, row.GetFieldAsZDecimal(field1.Name, 2));
			AssertEquals("Field 2", new ZDateTime(2006, 2, 8), row.GetFieldAsZDateTime(field2.Name, Constants.DateTimeFormat));
			AssertEquals("Field 3", "5 L?>*evel 2 Cote Ro", row.GetField(field3.Name));
			AssertEquals("Field 4", 69, row.GetFieldAsZInt(field4.Name));
			AssertEquals("Field 5", "5Level2CoteRoad", row.GetField(field5.Name));
		}
	}
}
