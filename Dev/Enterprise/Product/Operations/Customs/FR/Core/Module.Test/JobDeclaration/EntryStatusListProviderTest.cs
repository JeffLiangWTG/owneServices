using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	public class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var provider = new EntryStatusListProvider();
			AssertEquals(new EntryStatusDescriptionCodeList(), provider.EntryStatusList(new BusinessObjectFactory(), Core.Constants.CountryCodes.France, JobMessageTypeList.Codes.Import));

			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, true))
			{
				RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.France, new DeltaIEImportCusEntryStatusList().GetAllCodes(), Array.Empty<string>());
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, true))
			{
				RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.France, new DeltaIEImportCusEntryStatusList().GetAllCodes(), Array.Empty<string>());
			}
		}
	}
}
