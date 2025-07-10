using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaManifestHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var strategy = new AsycudaManifestHeaderProcessTaskLoadStrategy();
				var expectedType = ObjectFactory.GetType<Integration.Customs.ASYCUDA.IAsycudaManifestHeaderProcessTask>();
				var header = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
				AssertNull(strategy.GetTypeForLoad(AsycudaManifestHeaderSchema.Constants.Prefix, ZGuid.Empty, Factory));
				AssertNull(strategy.GetTypeForLoad(AsycudaManifestHeaderSchema.Constants.Prefix, ZGuid.Invalid, Factory));
				AssertEquals(expectedType, strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));
				AssertNull(strategy.GetTypeForLoad("Z@", header.PK, Factory));

				header = (BusinessObject)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
				AssertEquals(ObjectFactory.GetType<Integration.Customs.ZA.IAsycudaManifestHeaderProcessTask>(), strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));

				header = (BusinessObject)Factory.New<Integration.Customs.EU.ITemporaryStorageHeader>();
				AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.ITemporaryStorageHeaderProcessTask>(), strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));

				header = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>();
				AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeaderProcessTask>(), strategy.GetTypeForLoad(header.TablePrefix, header.PK, Factory));
			}
		}
	}
}
