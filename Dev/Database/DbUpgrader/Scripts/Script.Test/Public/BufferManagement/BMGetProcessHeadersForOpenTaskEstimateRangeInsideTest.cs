using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(BMGetProcessHeadersForOpenTaskEstimateRangeInside))]
	sealed class BMGetProcessHeadersForOpenTaskEstimateRangeInsideTest : DbCreateScriptTest
	{
		// This function is tested at the business layer: Enterprise.BufferManagement.Module.Test.OpenTaskEstimateRangeFilterTest
	}
}

