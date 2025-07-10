using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;

namespace Enterprise.Customs.FR.Module.TempStorageRegister.Testing
{
	[TestedType(typeof(TempStorageRegisterModule))]
	class TempStorageRegisterModuleTest : ZModuleBasherTest
	{
		public void TestGetNewController()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertType<TempStorageRegisterController>(module.GetNewController());
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				var collection = module.GridCollection;
				AssertType<Business.CusTempStorage.CusTempStorageRegHeaderCollection>("Type", collection);

				var istRegister = Factory.New<CusTempStorageRegHeader>();
				istRegister.SRH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
				Assert(istRegister.MatchesFilter(collection.CompleteFilter));

				var stoRegister = Factory.New<CusTempStorageRegHeader>();
				stoRegister.SRH_AppCode = FRConstants.TemporaryStorage.AppCodeSTO;
				Assert(stoRegister.MatchesFilter(collection.CompleteFilter));
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertEquals(Env.Security.FRTempStorageRegister, module.SecurityCheckpoint);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertType<TempStorageRegisterFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.TempStorageRegister;

		protected override string CountryCode => Core.Constants.CountryCodes.France;

		public override void TestModuleShowsAndCanSearch()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header.SRH_Reference = "Reference";
			header.SRH_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}
	}
}
