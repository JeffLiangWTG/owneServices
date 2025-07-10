using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.FR.Business.EntryStatusListProvider.EntryStatusListCore", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(new EntryStatusDescriptionCodeList());

				var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
				var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
				if (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.GetFallBackValueAtAllLevels(currentCompanyPK, currentBranchPK, Guid.Empty))
				{
					result.AddRange(new DeltaIEImportCusEntryStatusList());
				}
				return result;
			});
		}
	}
}
