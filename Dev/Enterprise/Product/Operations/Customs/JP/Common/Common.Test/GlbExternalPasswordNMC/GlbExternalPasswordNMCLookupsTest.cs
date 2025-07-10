using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(GlbExternalPasswordNMCLookups))]
	sealed class GlbExternalPasswordNMCLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPasswordStatusList()
		{
			var credential = Factory.New<GlbExternalPasswordNMC>();
			AssertEquals(credential.Lookups.PasswordStatusList, new CodeDescriptionPairList());
		}
	}
}
