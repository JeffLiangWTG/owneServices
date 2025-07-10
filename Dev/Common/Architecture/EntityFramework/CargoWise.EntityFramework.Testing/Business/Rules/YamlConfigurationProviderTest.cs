using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Business.Rules.Testing
{
	sealed class YamlConfigurationProviderTest : TestCase
	{
		const string YamlPath = "yamls";

		public void TestConfigurationKeyReturnsTableNameWithAnI()
		{
			var configProvider = new YamlConfigurationProvider(YamlPath);
			var mappings = new Dictionary<string, string>
			{
				{ "IGlbStaff", "IGlbStaff" },
				{ "IForwardingShipment", "IJobShipment" },
				{ "IOrgHeader", "IOrgHeader" },
				{ "IGlbBranch", "IGlbBranch" },
			};

			CombineAssertions(() =>
			{
				foreach (var kvp in mappings)
				{
					AssertEquals(kvp.Value, configProvider.GetConfigurationKey(ObjectFactory.GetType(kvp.Key)));
				}
			});
		}

		public void TestLoadFromFiles()
		{
			var testDir = Temp.GetNewTempSubdirectory();
			var dummyName = "IDummy";

			try
			{
				File.WriteAllText(Path.Combine(testDir, dummyName + ".yaml"),
	@"properties:
  Z0_Number:
  - ruleId: 0c34a592-07c5-4eb0-ad14-3d7a8e82ff1d
    className: CargoWise.EntityFramework.Testing.TestValidationMethods, CargoWise.EntityFramework.Testing
    methodName: GreaterThan
    arguments:
    - name: a
      value: <Z0_Number>
    - name: b
      value: 0
    ruleType: VAL
    propertyType: CargoWise.Glow.Infrastructure.Domain.RuleValidationResult
");
				var configs = new YamlConfigurationProvider(testDir);
				var result = configs.GetRuleConfiguration().Result;
				var dummy = result["IDummy"];

				AssertEquals("Should have loaded the one rule", 1, dummy.Properties.Count);

				var rule = dummy.Properties["Z0_Number"].Single();
				AssertEquals("Should be validation", "VAL", rule.RuleType);
				AssertEquals("Should have the two args", 2, rule.Arguments.Count());
			}
			finally
			{
				Directory.Delete(testDir, true);
			}
		}
	}
}
