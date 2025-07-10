using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;

namespace Enterprise.TimeEngineScheduler.ServiceTask.Test
{
	[TestedType(typeof(SchedulerActionAttribute))]
	sealed class SchedulerActionAttributeTest : AssemblyMetaDataAttributeTestCase<SchedulerActionAttribute>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.Code = "Code";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Code = "Code";
			Assert(attribute1.Equals(attribute2));

			attribute1.Description = "Description";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Description = "Description";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
