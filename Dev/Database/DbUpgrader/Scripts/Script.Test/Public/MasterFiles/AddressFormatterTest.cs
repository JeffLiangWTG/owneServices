using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(AddressFormatter))]
	class AddressFormatterTest : DbCreateScriptTest
	{
		public void TestAddressFormatter()
		{
			Action<string, string, string, string, string, string> assertJobDocAddress = (address1, address2, city, state, postCode, expectedDocAddress) =>
			{
				var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT Address FROM dbo.AddressFormatter('{0}', '{1}', '{2}', '{3}', '{4}', 'Australia')", address1, address2, city, state, postCode));
				AssertEquals(expectedDocAddress, result.Rows[0]["Address"]);
			};

			assertJobDocAddress("Add1-1", "Add1-2", "City1", "State1", "1111", "Add1-1\nAdd1-2\nCity1, State1 1111\nAustralia");

			assertJobDocAddress("Add2-1", "", "", "", "", "Add2-1\nAustralia");
			assertJobDocAddress("Add3-1", "Add3-2", "", "", "", "Add3-1\nAdd3-2\nAustralia");

			assertJobDocAddress("Add4-1", "", "", "State2", "", "Add4-1\nState2 \nAustralia");
			assertJobDocAddress("Add5-1", "", "", "", "2222", "Add5-1\n2222\nAustralia");
			assertJobDocAddress("Add6-1", "", "", "State3", "3333", "Add6-1\nState3 3333\nAustralia");
			assertJobDocAddress("Add7-1", "", "City2", "State4", "", "Add7-1\nCity2, State4 \nAustralia");
			assertJobDocAddress("Add8-1", "", "City3", "", "4444", "Add8-1\nCity3, 4444\nAustralia");
			assertJobDocAddress("Add9-1", "", "City4", "State5", "5555", "Add9-1\nCity4, State5 5555\nAustralia");
		}
	}
}

