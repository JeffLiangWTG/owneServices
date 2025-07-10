using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.DataTransfer.Testing;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DFD.Business.Import.Testing
{
	class USSupplierDataRowTest : SharedFlatFileDataRowTest
	{
		public void TestProperties()
		{
			string dataLine = "AHAD JUTE MILLS LTD,BDAHAJUTJES,BALIADANGA,JESSORE,121212,BD,09/30/2009";
			var dataRow = new USSupplierDataRow(new FlatFileDataRow(new OCsvLine(dataLine).FieldValues));
			AssertEquals("AHAD JUTE MILLS LTD", dataRow.MFName);
			AssertEquals("BDAHAJUTJES", dataRow.ManufacturerID);
			AssertEquals("BALIADANGA", dataRow.MFADDR);
			AssertEquals("JESSORE", dataRow.MFCITY);
			AssertEquals("121212", dataRow.MFZIP);
			AssertEquals("BD", dataRow.MFCTCD);
			AssertEquals(new ZDateTime(2009, 9, 30), dataRow.LastEntryDate);
		}
	}
}
