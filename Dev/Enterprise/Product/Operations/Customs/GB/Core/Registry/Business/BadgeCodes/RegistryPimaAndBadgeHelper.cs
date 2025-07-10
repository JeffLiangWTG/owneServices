using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Registry
{
	public static class RegistryPimaAndBadgeHelper
	{
		public static ZGuid GetPrimaryBranchPkFromRegistryBasedOnPima(ZString recipientPimaFromInterchange, BusinessObjectFactory factory, bool useCompanyFieldNotPimaField = false)
		{
			var gbToSet = GlbBranch.CurrentBranch.PK;
			try
			{
				var companies = new GlbCompany.Loader(factory).LoadCompanies(Core.Constants.CountryCodes.UnitedKingdom);
				gbToSet = GlbBranch.FindAnyBranchInSameCountry(factory, RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.UnitedKingdom))?.PK ?? gbToSet;
				foreach (var company in companies)
				{
					foreach (CredentialsSetting credential in GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
					{
						var credentialFieldToCompare = useCompanyFieldNotPimaField ? credential.Company : credential.PIMA;
						if (credentialFieldToCompare == recipientPimaFromInterchange)
						{
							foreach (var branch in company.ActiveBranches)
							{
								var existingBadgesForThisBranch = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
								if (existingBadgesForThisBranch != null)
								{
									foreach (BadgeCodeSetting badgeCodeSetting in existingBadgesForThisBranch)
									{
										if (badgeCodeSetting.BadgeCode == credential.BadgeCode
											&& (badgeCodeSetting.IsPrimaryBadgeForBranch || !(from BadgeCodeSetting b in existingBadgesForThisBranch where b.IsPrimaryBadgeForBranch select b).Any())  // allows users to no badges flagged as primary
											)
										{
											return branch.PK;
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }
			return gbToSet;
		}

		/// <summary>
		/// Gets only proper CHIEF-known PIMAs, i.e. full sheds and simple agents.
		/// </summary>
		public static CodeDescriptionPairList GetProfilesKnownToChiefList(GlbBranch glbBranch, BusinessObjectFactory factory, bool hasShedLicence, bool showDepShedsToo, bool showExportPimasOnly)
		{
			var list = factory.GetCachedValue("CusHawbLookup+Profiles" + ((glbBranch == null || glbBranch.PK.IsEmpty) ? "NoBranch" : glbBranch.PK.ToStringKey() + hasShedLicence + showDepShedsToo + showExportPimasOnly), delegate
			{
				var listCached = new CodeDescriptionPairList();
				foreach (BadgeCodeSetting badge in GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, glbBranch.PK.ToGuid(), Guid.Empty))
				{
					if (!showExportPimasOnly || (showExportPimasOnly && badge.Direction != BadgeDirectionList.Codes.IMP))
					{
						if (badge.CSPCode == GatewayList.Codes.CCSUKviaNTMsgGW)
						{
							var credential = CredentialsSetting.GetCredentialsForBadge(badge.BadgeCode, glbBranch.GB_GC);
							if (credential != null)
							{
								AddCcsukCredentialToList(glbBranch, hasShedLicence, showDepShedsToo, listCached, badge, credential);
							}
						}
					}
				}
				return listCached;
			});
			return list;
		}

		public static void AddCcsukCredentialToList(GlbBranch glbBranch, bool hasShedLicence, bool showDepShedsToo, CodeDescriptionPairList list, BadgeCodeSetting badge, CredentialsSetting credential, bool showPIMA = true)
		{
			var primaryBadgeSuffix = "";
			if (glbBranch == GlbBranch.CurrentBranch && badge.IsPrimaryBadgeForBranch)
			{
				primaryBadgeSuffix = " ***";
			}

			var agentOrShedorDep = "";
			if (credential.IsDEPOperator)
			{
				agentOrShedorDep = "DEP shed";
			}
			else if (credential.IsCcskAgent)
			{
				agentOrShedorDep = "Agent";
			}
			else if (credential.IsCcskShed)
			{
				agentOrShedorDep = "Shed";
			}
			if (credential.IsCcskAgent || (credential.IsCcskShed && hasShedLicence && !credential.IsDEPOperator) || (credential.IsDEPOperator && showDepShedsToo))
			{
				var code = showPIMA ? credential.Printer : badge.BadgeCode;  //printer=PIMA
				list.AddPairIfNotExist(code, string.Format(CultureInfo.CurrentCulture, "{0} {1}{2}", agentOrShedorDep, badge.BadgeCode, primaryBadgeSuffix));
			}
		}
	}
}
