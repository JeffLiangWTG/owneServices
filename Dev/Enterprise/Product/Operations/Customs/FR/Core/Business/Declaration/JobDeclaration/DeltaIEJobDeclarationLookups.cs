using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobDeclarationLookups : JobDeclarationLookups
	{
		public DeltaIEJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetDeltaModeList()
		{
			return new CodeDescriptionPairList();
		}

		public override CodeDescriptionPairList DefermentAccountNumberList => GetGuaranteeNumberList(DefermentCustomsGuarantees);

		public override CodeDescriptionPairList CustomsGuaranteeNumberList => GetGuaranteeNumberList(CODCustomsGuarantees);

		CodeDescriptionPairList GetGuaranteeNumberList(CusGuaranteeHeader[] guarantees)
		{
			var result = new CodeDescriptionPairList();
			var parent = Parent;
			var isImport = parent.IsImport;

			foreach (var guarantee in guarantees)
			{
				if (isImport)
				{
					if (guarantee.CPH_OH_PermitHolder == parent.Importer?.PK)
					{
						result.Insert(0, new CodeDescriptionPair(guarantee.CPH_Number.ToString(), $"{nameof(parent.Importer)}, {parent.Importer?.OH_FullName}"));
					}
					else if (guarantee.CPH_OH_PermitHolder == parent.Declarant?.Header?.PK)
					{
						result.AddPair(guarantee.CPH_Number.ToString(), $"{nameof(parent.Declarant)}, {parent.Declarant?.Header?.OH_FullName}");
					}
				}
			}
			return result;
		}

		public override CodeDescriptionPairList ProfileList
		{
			get
			{
				var cacheKey = "FR.JobDeclarationLookups.ProfileList" + Declaration.JE_ApplicationCode + Declaration.JE_MessageType + GetActualClientDeclarantAndRepresentativeCacheKey();
				return Factory.GetCachedValue(cacheKey, () => {
					var result = Declaration.DeltaAccounts.Aggregate(new CodeDescriptionPairList(), (list, account) =>
					{
						var headerPk = account.CZ_OH;
						var orgName = ZString.Empty;
						if (headerPk == Declaration.Importer?.PK)
						{
							orgName = (NoResString)"Importer ";
						}
						else if (headerPk == Declaration.Supplier?.PK)
						{
							orgName = (NoResString)"Supplier ";
						}
						else if (headerPk == Declaration.Declarant?.Header.PK)
						{
							orgName = (NoResString)"Declarant ";
						}
						else if (headerPk == Declaration.Representative?.Header.PK)
						{
							orgName = (NoResString)"Representative ";
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

		ZString GetActualClientDeclarantAndRepresentativeCacheKey()
		{
			return System.FormattableString.Invariant($"{GetActualClientAndDeclarantCacheKey()},{Declaration.JE_OA_Representative}");
		}

		public override ZString DataGroupingForVATCANA => Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;
	}
}
