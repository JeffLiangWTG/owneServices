using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.DataTransfer.Testing;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DFD.Business.Import.Testing
{
	class USOrganisationDataRowTest : SharedFlatFileDataRowTest
	{
		public void TestProperties()
		{
			string dataLine = "264520,MANO0,MASERATI NORTH AMERICA (PARTS),22-377846300,856,460324340,700000,8,3,,0,010603,03/15/2010,0170CA10020072";
			var dataRow = new USOrganisationDataRow(new FlatFileDataRow(new OCsvLine(dataLine).FieldValues));
			AssertEquals("264520", dataRow.LegacyCode);
			AssertEquals("MANO0", dataRow.CUCode);
			AssertEquals("MASERATI NORTH AMERICA (PARTS)", dataRow.CUName);
			AssertEquals("22-377846300", dataRow.EIN);
			AssertEquals("856", dataRow.SuretyCode);
			AssertEquals("460324340", dataRow.BondNumber);
			AssertEquals(700000m, dataRow.BondAmount);
			AssertEquals("8", dataRow.BondType);
			AssertEquals("3", dataRow.CustomsStatementType);
			AssertEquals(ZString.Empty, dataRow.POA);
			AssertEquals(0m, dataRow.POA_Exp);
			AssertEquals("010603", dataRow.ACHPayerUnit);
			AssertEquals(new ZDateTime(2010, 3, 15), dataRow.MaxImpData);
			AssertEquals("0170CA10020072", dataRow.LastSID);
		}
	}
}
