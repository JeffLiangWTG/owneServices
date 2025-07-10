using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS332DeclarationTypePartiesProviderTest : DataProviderTestCase<TS332DeclarationTypePartiesProvider>
	{
		public void TestPresenterID()
		{
			AssertEquals("PresenterID=>EOR", "IE987654321", Provider.PresenterID);
		}

		public void TestRepresentative()
		{
			AssertType<TSRepresentativeProvider>(Provider.Representative);
			AssertEquals("ID=>EOR", "IE123456789", Provider.Representative.ID);
			AssertEquals("Status", "2", Provider.Representative.Status);
		}

		protected override TS332DeclarationTypePartiesProvider GetProvider() => TS332DeclarationTypePartiesProvider.New(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			header.AMA_OA_Representative = representative.MainAddress.PK;
			header.AMA_AgentType = RepresentativeStatusCodeList.Codes._2;

			presenter = Factory.NewWithValidTestData<OrgHeader>();
			presenter.CustomsCodes.AddNew("EOR", "IE987654321", "IE");
			header.AMA_OA_Presenter = presenter.MainAddress.PK;
		}
		TemporaryStorageHeader header;
		OrgHeader representative;
		OrgHeader presenter;
	}
}
