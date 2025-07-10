using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.ECU.ConsolExport.Testing
{
	public class ECUFlatFileDataRowTest : TestCase
	{
		public void TestConstructor()
		{
			ZString headerValue = "HEADER";
			ZString fieldValue = "FIELD";
			ECUFlatFileDataRow record = new ECUFlatFileDataRow(headerValue, fieldValue);
			AssertNotNull("Record should not be null", record);
			AssertEquals("Field Count", 2, record.FieldCount);
		}
	}
}
