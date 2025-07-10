using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business
{
	public class GlbExternalPasswordLookups_GB : GlbExternalPasswordLookups
	{
		public GlbExternalPasswordLookups_GB(GlbExternalPassword_GB parent)
			: base(parent)
		{
		}

		GlbCompany Company => Factory.Load<GlbCompany>(Parent.GP_GC);

		public virtual CodeDescriptionPairList BadgeCodes
		{
			get
			{
				var company = Company;
				return company == null ? new CodeDescriptionPairList() : company.Factory.GetCachedValue("GBExternalPasswordBadgeCodes_" + company.GC_Code, () =>
				{
					var codeDescriptionPairList = new CodeDescriptionPairList();
					foreach (var branch in company.Branches)
					{
						var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
						badgeCodeSettings.OfType<BadgeCodeSetting>().ToList().ForEach(x => codeDescriptionPairList.AddPairIfNotExist(x.BadgeCode, x.BadgeCode));
					}
					return codeDescriptionPairList;
				});
			}
		}

		public virtual CodeDescriptionPairList EORIs
		{
			get
			{
				var company = Company;
				return company == null ? new CodeDescriptionPairList() : company.Factory.GetCachedValue("GBExternalPasswordEORIs_" + company.GC_Code, () =>
				{
					var eoris = new CodeDescriptionPairList();
					foreach (var pair in company.Branches.Select(x => x.OrgProxy).Concat(new[] { company.OrgProxy }).Select(oh => new { Org = oh, Eori = EuEoriProviderAndValidator.GetEuIdentificationNumber(oh) }).Distinct())
					{
						eoris.AddPair(FormattableString.Invariant($"{pair.Eori}"), FormattableString.Invariant($"{pair.Eori} ({pair.Org?.OH_Code} / {pair.Org?.OH_FullName})"));
					}
					return eoris;
				});
			}
		}
	}
}
