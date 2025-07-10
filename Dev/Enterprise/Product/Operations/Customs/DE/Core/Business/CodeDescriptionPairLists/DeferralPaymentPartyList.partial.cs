using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.DE.Business.CodeDescriptionPairLists
{
	public partial class DeferralPaymentPartyList
	{
		public static CodeDescriptionPairList GetDeferralAccountNumberList(ZString party, JobDeclaration declaration)
		{
			var result = new CodeDescriptionPairList();

			var deferralParty = GetDeferralParty(party, declaration);
			foreach (var orgCusAccount in deferralParty.GetDefermentAccounts().DistinctBy(x => Invariant($"{x.CZ_Code}|{x.CZ_Account}")))
			{
				var accountCode = orgCusAccount.CZ_Code;
				if (!(OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity == accountCode && !declaration.IsDeclarantEntitledToClaimBackVAT))
				{
					result.AddPair(orgCusAccount.CZ_Account, Res.GetString("89d3e593-faf1-4691-941b-fd53f0657d8a", "Account Type: {0}", accountCode));
				}
			}

			result.Sort();

			return result;
		}

		static OrgHeader GetDeferralParty(ZString partyType, JobDeclaration declaration)
		{
			OrgHeader org = null;

			switch (partyType)
			{
				case Codes.Declarant:
					org = declaration?.DeclarantAddress?.Header;
					break;
				case Codes.DefermentParty:
					org = declaration?.DefermentPartyDocAddress.Organisation;
					break;
				case Codes.Representative:
					org = declaration?.Representative?.Header;
					break;
				case Codes.RepresentedParty:
					org = declaration?.BuyingAgentAddress?.Header;
					break;
			}

			return org;
		}
	}
}
