using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	public static class MawbTestHelper
	{
		public static void MakeBadge(ZString badgeCode, ZString csp, bool makeCredentialToo = true, string company = "", string applicationCode = "")
		{
			MakeBadge(badgeCode, csp, "CUKFFW98000", makeCredentialToo, company, applicationCode: applicationCode);
		}

		public static void MakeExportTestBadge(ZString badgeCode, ZString direction, string applicationCode = "")
		{
			MakeBadge(badgeCode, GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, badgeDirection: direction, applicationCode: applicationCode);
		}

		public static void MakeBadge(ZString badgeCode, ZString csp, string pimaPrefix, bool makeCredentialToo = true, string company = "", bool isPrimaryBadge = false, bool isMartimeLoader = false, string badgeDirection = BadgeDirectionList.Codes.Both, string portCode = "", string mucrCalculationMode = "", string applicationCode = "")
		{
			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badge = new BadgeCodeSetting();
			badge.ApplicationCode = applicationCode;
			badge.CSPCode = csp;
			badge.BadgeCode = badgeCode;
			badge.IsPrimaryBadgeForBranch = isPrimaryBadge;
			badge.Direction = badgeDirection;
			badge.RL_PortCode = portCode;
			badge.MasterUcrCalculationMode = mucrCalculationMode;
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			if (makeCredentialToo)
			{
				var credential = new CredentialsSetting();
				credential.BadgeCode = badge.BadgeCode;
				credential.Company = company;
				credential.Printer = pimaPrefix + badgeCode;
				credential.IsMaritimeLoader = isMartimeLoader;
				if (csp == GatewayList.Codes.CNS_CUSDECOnly || csp == GatewayList.Codes.MCP_CUSDECOnly)
				{
					credential.Username = "X";
					credential.Password = "X";
				}
				var allCreds = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				allCreds.Add(credential);
				GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);
			}
		}

		public static void MakeDepBadge(ZString badgeCode, ZString csp, bool makeCredentialToo = true, string applicationCode = "")
		{
			MakeBadge(badgeCode, csp, "CUKAIR98LHR", makeCredentialToo, applicationCode: applicationCode);
		}
	}
}
