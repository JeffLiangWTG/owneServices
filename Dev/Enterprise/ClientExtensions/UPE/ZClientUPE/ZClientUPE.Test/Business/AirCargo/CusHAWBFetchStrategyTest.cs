using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class CusHAWBFetchStrategyTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestFetchForLoad()
		{
			UPECusHAWB uPECusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			UPECusMAWB uPECusMAWB = Factory.NewWithValidTestData<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "08122222222";
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			UPECusMAWB unRelatedUPECusMAWB = Factory.NewWithValidTestData<UPECusMAWB>();
			Factory.Save();
			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();
			uPECusHAWB = cleanFactory.Load<UPECusHAWB>(uPECusHAWB.PK);
			unRelatedUPECusMAWB = cleanFactory.Load<UPECusMAWB>(unRelatedUPECusMAWB.PK);
			ZQuery uPECusMAWBFilter = new ZQuery(CusMAWBSchema.PK, uPECusMAWB.PK);
			uPECusMAWBFilter.FetchOnlyFromLocalCache = true;
			uPECusMAWB = cleanFactory.LoadTop1<UPECusMAWB>(uPECusMAWBFilter);
			AssertEquals("08122222222", uPECusMAWB.CM_MAWB);
		}
	}
}
