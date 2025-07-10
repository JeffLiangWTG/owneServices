using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportPartyIDProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportPartyIDProvider>
	{
		public void TestConstructor_OrgAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null", ImportPartyIDProvider.NewOrNull(null));
				AssertNotNull("Not Null", ImportPartyIDProvider.NewOrNull(orgAddress));
			});
		}

		public void TestEoriNumber()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			AssertEquals("GR123456789", Provider.EoriNumber);
		}

		public void TestEoriNumber_Missing()
		{
			orgHeader.DeleteSingleEORINumber();
			AssertEquals(string.Empty, Provider.EoriNumber);
		}

		public void TestEoriNumber_Multiple()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", Core.Constants.CountryCodes.Greece);
			AssertEquals("* multiple EOR *", Provider.EoriNumber);
		}

		public void TestEoriBranchSuffix()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			AssertEquals("0001", Provider.EoriBranchSuffix);
		}

		public void TestEoriBranchSuffix_FallBack()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			orgHeader.DeleteSingleEORIBranch();
			AssertEquals("0000", Provider.EoriBranchSuffix);
		}

		public void TestEoriBranchSuffix_MissingEoriNumber()
		{
			orgHeader.DeleteSingleEORINumber();
			AssertNull(Provider.EoriBranchSuffix);
		}

		public void TestTCUNumber()
		{
			AssertNull(Provider.TCUNumber);
		}

		protected override ImportPartyIDProvider GetProvider() => ImportPartyIDProvider.NewOrNull(orgAddress);

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "123456789", Core.Constants.CountryCodes.Greece, "0001");
		}
		OrgHeader orgHeader;
		OrgAddress orgAddress => orgHeader.MainAddress;
	}
}
