using Enterprise.Client.EDI;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace ZClientEDI.Business.Testing
{
	[TestedType(typeof(EDIAutoLoggedTablesDefaultConfigValues))]
	public class EDIAutoLoggedTablesDefaultConfigValuesTest : TestCase
	{
		public void TestGetDefaultConfigurationValues()
		{
			var instance = AutoLoggedTablesDefaultConfigValues.Instance;
			AssertType(typeof(EDIAutoLoggedTablesDefaultConfigValues), instance);

			Assert(instance.GetDefaultConfigurationValues().ContainsKey(IncidentManagementGroupSchema.Constants.TableName));

			var configuration = instance.GetDefaultConfigurationValues()[IncidentManagementGroupSchema.Constants.TableName];
			AssertEquals(configuration.IsEnabledForADD, false);
			AssertEquals(configuration.IsEnabledForEDT, false);
			AssertEquals(configuration.IsEnabledForDEL, false);
		}
	}
}
