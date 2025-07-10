using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Test.OrgHeaderWrapper
{
	class OrganisationTests : TestCaseWithFactory
	{
		void CheckEmptyAddress(string type, IOrganisation io)
		{
			CombineAssertions("No Nulls for " + type, () =>
			{
				AssertEquals("CountryCode", ZString.Empty, io.CountryCode);
				AssertEquals("City", ZString.Empty, io.City);
				AssertEquals("EoriCode", ZString.Empty, io.EoriCode);
				AssertEquals("ShortCode", ZString.Empty, io.ShortCode);
				AssertEquals("Name", ZString.Empty, io.Name);
				AssertEquals("PostCode", ZString.Empty, io.PostCode);
				AssertEquals("Street", ZString.Empty, io.Street);
			});
		}

		public void TestOrganisationBlankAddresses()
		{
			IOrganisation io = new OrgHeaderOrganisation(Factory.New<OrgAddress>());
			CheckEmptyAddress("OrgHeaderOrganisation", io);

			io = new JobDocAddressOrganisation(Factory.New<JobDocAddress>());
			CheckEmptyAddress("JobDocAddressOrganisation", io);

			io = OrganisationProvider.Get((OrgHeader)null);
			CheckEmptyAddress("OrgHeader", io);

			io = OrganisationProvider.Get((OrgAddress)null);
			CheckEmptyAddress("OrgAddress", io);

			io = OrganisationProvider.Get((Enterprise.MasterFiles.Integration.IDocAddress)null);
			CheckEmptyAddress("IDocAddress", io);
		}
	}
}
