using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TemporaryStorageModule))]
	class TemporaryStorageModuleTest : EU.TemporaryStorage.Module.Testing.TemporaryStorageModuleAbstractTest
	{
		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			var jobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			jobHeader.SJH_AppCode = "IST";
			jobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			var storageDec = CusTempStorageDec.New(jobHeader);
			storageDec.CusTempStorageLines.AddNew();
			Factory.Save();

			base.TestModuleShowsAndCanSearch();
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TemporaryStorage;

		protected override bool HasController() => true;
	}
}
