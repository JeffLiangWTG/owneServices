using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.Testing
{
	[TestedType(typeof(csfn_JobShipmentWithDirection))]
	class csfn_JobShipmentWithDirectionEDWTest : BiCreateScriptTest
	{
		public void TestSQLHasNotChanged()
		{
			var expectedHashedCode = "91C06EC229D999DF196E2719DD9A70C9C06A0943068B10E89A8BF85F2A63F990";
			var sqlFunction = new csfn_JobShipmentWithDirection();
			var actualHashedCode = GetHashString(sqlFunction.Text);
			AssertEquals("A version of this function exists in the Odyssey database(/CargoWise.DbUpgrader/src/Scripts/Scripts.Definitions/Freight/Shipment/csfn_JobShipmentWithDirection.sql), please update it and recalculate the hash.", expectedHashedCode, actualHashedCode);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
