using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TSRepresentativeProviderTest : DataProviderTestCase<TSRepresentativeProvider>
	{
		public void TestStatus()
		{
			AssertEquals("When AMA_AgentType is empty", ZString.Empty, Provider.Status);

			header.AMA_AgentType = RepresentativeStatusCodeList.Codes._2;
			AssertEquals("When AMA_AgentType is 2", "2", GetProvider().Status);

			header.AMA_AgentType = RepresentativeStatusCodeList.Codes._3;
			AssertEquals("When AMA_AgentType is 3", "3", GetProvider().Status);
		}

		public void TestID()
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "B007");
			AssertEquals(string.Empty, GetProvider().ID);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			AssertEquals("IEE007", GetProvider().ID);
		}

		protected override TSRepresentativeProvider GetProvider() => TSRepresentativeProvider.New(header);

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgAddress = orgHeader.MainAddress;
			header = Factory.New<TemporaryStorageHeader>();
			header.AMA_OA_Representative = orgAddress.PK;
		}
		TemporaryStorageHeader header;
		OrgHeader orgHeader;
		OrgAddress orgAddress;
	}
}
