using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBFlagDetailsLookupsTest : TestCase
	{
		public void TestAuthReceivedByList()
		{
			UPECusHAWBFlagDetailsLookups lookups = new UPECusHAWBFlagDetailsLookups(null);
			AssertNotNull(lookups.AuthReceivedByList);
		}
	}
}
