using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.Testing
{
	[TestedType(typeof(FCLShipmentContainers))]
	class FCLShipmentContainersTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
