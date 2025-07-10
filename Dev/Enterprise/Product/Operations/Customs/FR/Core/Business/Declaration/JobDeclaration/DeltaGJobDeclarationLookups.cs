using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGJobDeclarationLookups : JobDeclarationLookups
	{
		public DeltaGJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetDeltaModeList()
		{
			var orgCusAccountDeltaGTypeList = new OrgCusAccountDeltaGTypeList();
			return Declaration.DeltaAccounts.Select(x => x.CZ_Type)
				.Distinct()
				.OrderBy(x => x)
				.Aggregate(new CodeDescriptionPairList(), (list, mode) =>
				{
					list.AddPair(mode, orgCusAccountDeltaGTypeList.GetDescriptionFromCode(mode));
					return list;
				});
		}

		public override CodeDescriptionPairList ProfileList
		{
			get
			{
				var cacheKey = "FR.JobDeclarationLookups.ProfileList" + Declaration.JE_ApplicationCode + Declaration.JE_MessageType + GetActualClientAndDeclarantCacheKey();
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = Declaration.DeltaAccounts.Aggregate(new CodeDescriptionPairList(), (list, account) =>
					{
						var headerPk = account.CZ_OH;
						ZString orgName;
						if (headerPk == Declaration.Importer?.PK)
						{
							orgName = (NoResString)"Importer ";
						}
						else if (headerPk == Declaration.Supplier?.PK)
						{
							orgName = (NoResString)"Supplier ";
						}
						else
						{
							orgName = (NoResString)"Declarant ";
						}
						orgName += account.Header.OH_FullName;
						var deltaMode = (NoResString)"Delta " + account.CZ_Type;
						var description = string.Join(", ", orgName, deltaMode, account.CZ_Issuer);
						list.AddPair(account.CZ_Account, description);
						return list;
					});
					result.Sort();
					return result;
				});
			}
		}
		public override ZString DataGroupingForVATCANA => Core.Constants.CountryCodes.France;
	}
}
