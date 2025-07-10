using System.Linq;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class EntryFeePaymentPartyUnderstander : EU.Business.Declaration.EntryFeePaymentPartyUnderstander
	{
		public EntryFeePaymentPartyUnderstander(EU.Business.Declaration.JobDeclaration declaration)
			: base(declaration)
		{
		}

		public override bool ShouldBrokerPayThisFee(string feeCode, string methodOfPayment, ILogger logger)
		{
			var methodOfPaymentOnDeclaration = Declaration.ZG_MethodOfPayment;
			if (methodOfPaymentOnDeclaration.IsEmpty)
			{
				return false; // If no Method of Payment captured on Declaration we don't want the Fees in the auto-rating.
			}
			else if (MethodOfPaymentHelper.RequireDeferralPaymentParty(methodOfPaymentOnDeclaration))
			{
				return ShouldBrokerPayThisFeeIfMethodOfPaymentIsEFGZ(feeCode);
			}
			else
			{
				return ShouldBrokerPayThisFeeIfMethodOfPaymentIsNotEFGZ(); // For all other than E, F, G or Z we determine based on DefermentPartyDocAddress (PaymentParty).
			}
		}

		bool ShouldBrokerPayThisFeeIfMethodOfPaymentIsNotEFGZ()
		{
			var defermentPartyPk = Declaration.DefermentPartyDocAddress?.OrganisationPK;
			var orgProxyPk = Declaration.Branch?.GB_OH_OrgProxy;

			return defermentPartyPk.HasValue && orgProxyPk.HasValue && defermentPartyPk.Value == orgProxyPk.Value;
		}

		bool ShouldBrokerPayThisFeeIfMethodOfPaymentIsEFGZ(string feeCode)
		{
			var orgProxyPk = Declaration.Branch?.GB_OH_OrgProxy;

			var party1 = GetAccountOrg(Declaration.JE_PaymentMethod);
			var isParty1Broker = party1?.PK == orgProxyPk;
			var party1AccountNumber = Declaration.JE_DefermentAccountNumber;

			var party2 = GetAccountOrg(Declaration.ZG_VATDeferType);
			var isParty2Broker = party2?.PK == orgProxyPk;
			var party2AccountNumber = Declaration.ZG_VATDeferNumber;

			var account1Type = GetAccountType(party1, party1AccountNumber);
			var account2Type = GetAccountType(party2, party2AccountNumber);

			var isVat = feeCode == Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat;

			if (party1 != null && !isParty1Broker && party2 != null && !isParty2Broker) // Two Deferment-Accounts captured and Client owns both -> No auto-rating.
			{
				return false;
			}

			if (isVat) // For VAT Account Types 10 and 20 are valid. If both are captured, 20 always wins.
			{
				return ShouldBrokerPayThisFeeIfMethodOfPaymentIsEFGZForVat(isParty1Broker, account1Type, isParty2Broker, account2Type);
			}
			else // For Duty Account Types 10 and 15 are valid. If both are captured, 15 always wins.
			{
				return ShouldBrokerPayThisFeeIfMethodOfPaymentIsEFGZForDuty(isParty1Broker, account1Type, isParty2Broker, account2Type);
			}
		}

		static bool ShouldBrokerPayThisFeeIfMethodOfPaymentIsEFGZForVat(bool isParty1Broker, string account1Type, bool isParty2Broker, string account2Type)
		{
			if (!isParty1Broker && account1Type == OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity) // 20
			{
				return false;
			}
			if (!isParty2Broker && account2Type == OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity) // 20
			{
				return false;
			}
			if (isParty1Broker && account1Type == OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity) // 20
			{
				return true;
			}
			if (isParty2Broker && account2Type == OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity) // 20
			{
				return true;
			}
			if (!isParty1Broker && account1Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonth) // 10
			{
				return false;
			}
			if (!isParty2Broker && account2Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonth) // 10
			{
				return false;
			}
			return true;
		}
		static bool ShouldBrokerPayThisFeeIfMethodOfPaymentIsEFGZForDuty(bool isParty1Broker, string account1Type, bool isParty2Broker, string account2Type)
		{
			if (!isParty1Broker && account1Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT) // 15
			{
				return false;
			}
			if (!isParty2Broker && account2Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT) // 15
			{
				return false;
			}
			if (isParty1Broker && account1Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT) // 15
			{
				return true;
			}
			if (isParty2Broker && account2Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT) // 15
			{
				return true;
			}
			if (!isParty1Broker && account1Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonth) // 10
			{
				return false;
			}
			if (!isParty2Broker && account2Type == OrgCusAccountCodeList.Codes.ImportDutiesOneMonth) // 10
			{
				return false;
			}
			return true;
		}

		OrgHeader GetAccountOrg(string partyType)
		{
			switch (partyType)
			{
				case DeferralPaymentPartyList.Codes.Declarant:
					return Declaration.Declarant?.Header;
				case DeferralPaymentPartyList.Codes.Representative:
					return Declaration.Representative?.Header;
				case DeferralPaymentPartyList.Codes.DefermentParty:
					return Declaration.DefermentPartyDocAddress?.Organisation;
				default:
					return null;
			}
		}

		static string GetAccountType(OrgHeader accountOrg, string accountNumber)
		{
			return accountOrg?
				.GetDefermentAccounts()
				.FirstOrDefault(x => x.CZ_Account == accountNumber)?
				.CZ_Code;
		}
	}
}
