using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestCusTempStorageJobHeaderLoadStrategyGetTypeForLoad()
		{
			var lvCompany = Factory.New<GlbCompany>();
			lvCompany.GC_Code = "123";
			lvCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			var lvBranch = lvCompany.Branches.AddNew();
			lvBranch.GB_Code = "456";
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var lvJob = (CusTempStorageJobHeader)Factory.New<Integration.Customs.EU.ICusTempStorageJobHeader>();
				lvJob.SJH_GB = lvBranch.PK;
				RunTypeTest(lvJob);
			}
		}

		void RunTypeTest(CusTempStorageJobHeader job)
		{
			var strategy = new CusTempStorageJobHeaderProcessTaskLoadStrategy();
			AssertNull(strategy.GetTypeForLoad(CusTempStorageJobHeaderSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertNull(strategy.GetTypeForLoad(CusTempStorageJobHeaderSchema.Constants.Prefix, ZGuid.Invalid, Factory));
			AssertEquals(typeof(CusTempStorageJobHeaderProcessTask), strategy.GetTypeForLoad(job.TablePrefix, job.PK, Factory));
			AssertNull(strategy.GetTypeForLoad("Z@", job.PK, Factory));
			job = Factory.New<CusTempStorageJobHeader>();
			AssertEquals(typeof(CusTempStorageJobHeaderProcessTask), strategy.GetTypeForLoad(job.TablePrefix, job.PK, Factory));
		}
	}
}
