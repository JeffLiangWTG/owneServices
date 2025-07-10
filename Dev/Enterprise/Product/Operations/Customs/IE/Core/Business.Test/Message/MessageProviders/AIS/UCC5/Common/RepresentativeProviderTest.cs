using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class RepresentativeProviderTest : DataProviderTestCase<RepresentativeProvider>
	{
		public void TestStatus()
		{
			declaration.JE_DeclarantType = ZString.Empty;
			AssertNull("Null Status", GetProvider().Status);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("Status 2 JE_DeclarantType", "2", GetProvider().Status);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("Status 3 JE_DeclarantType", "3", GetProvider().Status);
		}

		public void TestID()
		{
			AssertEquals("ID=>CustomsCodes EOR", "IE123456789", Provider.ID);
		}

		public void TestAddress()
		{
			representative.CustomsCodes.RemoveAndDeleteAll();
			var address = Provider.Address;
			AssertType<AddressWithNameProvider>(address);
			AssertSame("Cached", address, Provider.Address);
		}

		protected override void SetUp() => SetUpTestDataIfNeeded();

		protected override RepresentativeProvider GetProvider() => RepresentativeProvider.New(declaration);

		void SetUpTestDataIfNeeded()
		{
			if (representative == null)
			{
				declaration = Factory.New<JobDeclaration>();
				representative = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = representative.MainAddress;
				representative.OH_FullName = "Importer 1";
				orgAddress.Address1 = "Importer Address 1";
				representative.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
				declaration.JE_OA_Representative = representative.MainAddress.PK;
			}
		}

		JobDeclaration declaration;
		OrgHeader representative;
	}
}
