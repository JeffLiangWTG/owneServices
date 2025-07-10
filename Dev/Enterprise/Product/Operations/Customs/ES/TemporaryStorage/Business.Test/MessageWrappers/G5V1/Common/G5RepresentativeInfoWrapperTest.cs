using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5RepresentativeInfoWrapperTest : WrapperHelperTest<G5RepresentativeInfoWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("OrgAddress null", PartyNameWrapper.New((OrgAddress)null));
				var address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", G5RepresentativeInfoWrapper.New(address));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", G5RepresentativeInfoWrapper.New(address));
			});
		}

		public void TestStatus()
		{
			AssertEquals("Expected filled Status with fixed value 2", "2", wrapper.Status);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = G5RepresentativeInfoWrapper.New(orgAddress);
		}

		G5RepresentativeInfoWrapper wrapper;

		protected override G5RepresentativeInfoWrapper GetProvider() => wrapper;
	}
}
