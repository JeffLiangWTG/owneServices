using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(GlbExternalPasswordCUSLookups))]
	sealed class GlbExternalPasswordCUSLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportModeList()
		{
			var externalPassword = Factory.New<GlbExternalPasswordCUS>();
			AssertType<UserCodeSpecificTransportModeList>(externalPassword.Lookups.TransportModeList);
		}
	}
}
