using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentManagementGroupMessageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			AssertEquals(4, message.Lookups.Types.Count);
			Assert(message.Lookups.Types.Contains(new CodeDescriptionPair("OPN", "Opening")));
			Assert(message.Lookups.Types.Contains(new CodeDescriptionPair("INT", "Interim")));
			Assert(message.Lookups.Types.Contains(new CodeDescriptionPair("CLS", "Closing")));
			Assert(message.Lookups.Types.Contains(new CodeDescriptionPair("AUT", "Auto-reply")));
		}
	}
}
