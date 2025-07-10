using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(RepresentativeProvider))]
	class RepresentativeProviderTest : PartyProviderAbstractTest<RepresentativeProvider>
	{
		public void TestStatus()
		{
			AssertEquals(2, Provider.Status);
		}

		public void TestConstructorNull()
		{
			AssertNull(RepresentativeProvider.New(Factory.New<JobDocAddress>(), false));
		}

		protected override RepresentativeProvider CreateProvider(JobDocAddress address) => RepresentativeProvider.New(address, false);

		protected override string AddressType => "REP";

		protected override bool ExpectProviderIncludesAddress => false;
	}
}
