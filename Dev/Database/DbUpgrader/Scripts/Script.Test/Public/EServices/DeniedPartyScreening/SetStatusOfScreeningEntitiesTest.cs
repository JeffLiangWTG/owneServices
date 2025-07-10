using CargoWise.DbUpgrader.Scripts.Definitions.EServices.DeniedPartyScreening;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.EServices.DeniedPartyScreening.Testing
{
	[TestedType(typeof(SetStatusOfScreeningEntities))]
	class SetStatusOfScreeningEntitiesTest : DbCreateScriptTest
	{
		//Tested in Enterprise.DeniedPartyScreening.ServiceTasks.RescreeningServiceTask itself.
	}
}
