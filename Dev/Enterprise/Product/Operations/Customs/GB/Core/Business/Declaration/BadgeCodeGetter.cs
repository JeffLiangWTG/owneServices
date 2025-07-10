using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class BadgeCodeGetter
	{
		public static BadgeCodeGetter InstanceCachedFor(IConsolMessagingProvider consolMessagingProvider)
		{
			ZGuid branchPK = consolMessagingProvider.Branch?.PK ?? ZGuid.Empty;
			Guid branchGuid = (branchPK.IsEmpty || !branchPK.IsValid ? GlbBranch.CurrentBranch.PK : branchPK).ToGuid();
			return consolMessagingProvider.Factory.GetCachedValue(branchGuid.ToString(), delegate
			{ return new BadgeCodeGetter(branchGuid, consolMessagingProvider.Branch?.Company); });
		}

		BadgeCodeGetter(Guid branchGuid, GlbCompany company)
		{
			this.branchGuid = branchGuid;
			this.company = company;
		}
		readonly Guid branchGuid;
		readonly GlbCompany company;

		public BadgeCodeSetting GetFromBadgeCode(ZString badgeCode, ZString directionIMPorEXP)
		{
			return BadgeCodeSettings.FindByBadgeCode(badgeCode, directionIMPorEXP);
		}

		/// <summary>
		/// Gets badge code for this port and this direction (IMP/EXP)
		/// </summary>
		/// <param name="portCode"></param>
		/// <param name="importOrExport">IMP or EXP - allows us to use different default badges for import vs export (eg imports want to use badge-per-port, export always want to use ZPE)</param>
		/// <returns></returns>
		public BadgeCodeSetting GetFromPortCode(ZString portCode, ZString importOrExport)
		{
			return BadgeCodeSettings.FindByPortCode(portCode, importOrExport);
		}

		public CodeDescriptionPairList GetBadgeList(ZString importOrExport, ZString portCode, bool excludeCcsukShedsAndDeps = true)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (var badgeCode in GetBadgeCodes(importOrExport, portCode))
			{
				bool shouldAdd = true;
				if (excludeCcsukShedsAndDeps && company != null)
				{
					var credential = CredentialsSetting.GetCredentialsForBadge(badgeCode.BadgeCode, company.PK);
					if (credential != null && (credential.IsCcskShed || credential.IsDEPOperator))
					{
						shouldAdd = false;
					}
				}
				if (shouldAdd)
				{
					result.AddPairIfNotExist(badgeCode.BadgeCode, string.Empty);
				}
			}
			result.Sort();
			return result;
		}

		public BadgeCodeSetting[] GetBadgeCodes(ZString importOrExport, ZString portCode) => BadgeCodeSettings.OfType<BadgeCodeSetting>().Where(x => (importOrExport == x.Direction || importOrExport.IsEmpty || x.Direction == BadgeDirectionList.Codes.Both)
															&& (x.RL_PortCode.IsEmpty || x.RL_PortCode.Equals(portCode) || portCode.IsEmpty)).ToArray();

		BadgeCodeSettingCollection BadgeCodeSettings
		{
			get { return fbadgeCodeSettings ?? (fbadgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branchGuid, Guid.Empty)); }
		}
		BadgeCodeSettingCollection fbadgeCodeSettings;
	}
}
