using CargoWise.DbUpgrader.Scripts.Definitions.Core.Statistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Statistics.Testing
{
	[TestedType(typeof(vw_StatisticsMeasurementsExcludingChildren))]
	class vw_StatisticsMeasurementsExcludingChildrenTest : DbCreateScriptTest
	{
	}
}
