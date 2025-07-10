using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(CognosGroupingFlags))]
	class CognosGroupingFlagsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			CognosGroupingFlags groupingFlags = Factory.New<CognosGroupingFlags>();
			AssertEquals("Should disable company grouping by default", ClientCognosGroupingFlagsLookups.IntercompanyCodes.Exclude, groupingFlags.T4_Company);
		}
	}
}
