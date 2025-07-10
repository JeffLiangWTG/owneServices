using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class ComplianceSubTypeRule
	{
		public ComplianceSubTypeRule(BusinessObjectFactory factory, IEvaluateComplianceRule evaluateComplianceRule)
		{
			this.EvaluateComplianceRule = evaluateComplianceRule;
			Factory = factory;
		}

		readonly IEvaluateComplianceRule EvaluateComplianceRule;
		readonly BusinessObjectFactory Factory;

		IAccountingCountryFactory AccountingCountryFactory => accountingCountryFactory ?? (accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		IAccountingCountryFactory accountingCountryFactory;

		public ZString GetMatchingComplianceSubType(Guid invoiceBranchPk = new Guid())
		{
			var matchRule = FindMatchingComplianceSubTypeRule(invoiceBranchPk);
			var mathSubType = matchRule?.SubType ?? string.Empty;

			if (ApplySubTypeThresholdNotMet(matchRule))
			{
				mathSubType = matchRule.SubTypeThresholdNotMet;
			}

			return mathSubType;
		}

		ComplianceSubTypeAttributionRuleConfiguration FindMatchingComplianceSubTypeRule(Guid invoiceBranchPk)
		{
			var result = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value;
			if (invoiceBranchPk != Guid.Empty)
			{
				var complianceSubTypeAttributionRuleConfigurationCollection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, invoiceBranchPk, Guid.Empty);
				if (complianceSubTypeAttributionRuleConfigurationCollection.Any())
				{
					result = complianceSubTypeAttributionRuleConfigurationCollection;
				}
			}

			var allMatchingConfigs = ContainsNonCommentLines
				? result
					.Cast<ComplianceSubTypeAttributionRuleConfiguration>()
					.Where(IsTransactionMatchingComplianceRule)
				: Enumerable.Empty<ComplianceSubTypeAttributionRuleConfiguration>();

			return GetBestMatchBasedOnPrecedenceOrder(allMatchingConfigs.ToArray());
		}

		bool ApplySubTypeThresholdNotMet(IAccComplianceRule rule)
		{
			bool result = false;

			if (rule != null && rule.ThresholdApplies)
			{
				var thresholdAmount = (AccountingCountryFactory as IThresholdProvider)?.GetThresholdAmount();
				if (thresholdAmount.HasValue)
				{
					result = EvaluateComplianceRule.LocalTotalAmount < thresholdAmount;
				}
			}

			return result;
		}

		ComplianceSubTypeAttributionRuleConfiguration GetBestMatchBasedOnPrecedenceOrder(ComplianceSubTypeAttributionRuleConfiguration[] configs)
		{
			var useDefaultOrder = true;
			if (configs.Any(x => x.TaxInvoiceRule == TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly))
			{
				// If all tax ID is exclude ignore other overlaping rules
				configs = configs.Where(x => x.TaxInvoiceRule == TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly).ToArray();
			}

			var orderedConfigurations = configs.OrderBy(x => 1);
			var taxInvoiceRulePrecedenceProvider = CountryComplianceFactory.GetIComplianceSubTypeTaxInvoiceRulePrecedenceProvider(GlbCompany.CurrentCompany.Country.Code);
			if (taxInvoiceRulePrecedenceProvider != null)
			{
				var taxInvoiceRulePrecedenceList = taxInvoiceRulePrecedenceProvider.ComplianceSubTypeTaxInvoiceRulePrecedenceList();
				orderedConfigurations = orderedConfigurations.ThenBy(x => Array.IndexOf(taxInvoiceRulePrecedenceList, x.TaxInvoiceRule));
				useDefaultOrder = false;
			}

			var taxInvoiceRegistrationTypePrecedenceProvider = CountryComplianceFactory.GetIComplianceSubTypeTaxRegistrationTypePrecedenceProvider(GlbCompany.CurrentCompany.Country.Code);
			if (taxInvoiceRegistrationTypePrecedenceProvider != null)
			{
				var taxRegistrationTypePrecedenceList = taxInvoiceRegistrationTypePrecedenceProvider.ComplianceSubTypeTaxRegistrationTypePrecedenceList();
				orderedConfigurations = orderedConfigurations.ThenBy(x => Array.IndexOf(taxRegistrationTypePrecedenceList, x.TaxRegistrationType));
				useDefaultOrder = false;
			}

			var countryInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code);
			if (countryInfo is IComplianceSubTypeRuleSortByTaxRegistrationLocation)
			{
				orderedConfigurations = orderedConfigurations.ThenByDescending(x => x.TaxRegistrationLocationRule); //sort to make the empty location rule as the last one.
				useDefaultOrder = false;
			}

			if (countryInfo is IComplianceSubTypeRuleSortByParentTransactionSubType)
			{
				orderedConfigurations = orderedConfigurations.ThenByDescending(x => x.ParentTransactionSubType);
				useDefaultOrder = false;
			}

			if (useDefaultOrder)
			{
				orderedConfigurations = orderedConfigurations.OrderByDescending(x => x.OrganisationLocation)
										.ThenByDescending(x => x.TaxRegistrationType);
			}

			return orderedConfigurations.FirstOrDefault();
		}

		public bool IsTransactionMatchingRuleWithComplianceSubType(IAccComplianceRule rule)
		{
			return (rule.SubType.IsEmpty || rule.SubType == EvaluateComplianceRule.ComplianceSubType) && IsTransactionMatchingComplianceRule(rule);
		}

		bool IsTransactionMatchingComplianceRule(IAccComplianceRule rule)
		{
			var match = rule.Country == GlbCompany.CurrentCompany.Country.Code;
			match = match && ((rule.LedgerType.IsEmpty && EvaluateComplianceRule.EmptyLedgerMatchesAll) || rule.LedgerType == EvaluateComplianceRule.Ledger);
			match = match && ((rule.InvoiceType.IsEmpty && EvaluateComplianceRule.EmptyTransactionTypeMatchesAll) || rule.InvoiceType == EvaluateComplianceRule.TransactionType);
			match = match && EvaluateTaxInvoiceRule(rule);
			match = match && EvaluateDisbursementRule(rule);
			match = match && EvaluateOriginalRule(rule);
			match = match && EvaluateTaxRegistrationTypeRule(rule);
			match = match && EvaluateOrganisationLocationRule(rule);
			match = match && EvaluateSelfBillingRule(rule);
			match = match && EvaluateTaxRegistrationLocationRule(rule);
			match = match && EvaluateVATGroupRule(rule);
			match = match && EvaluateParentTransactionRule(rule);
			match = match && EvaluateExporterExemptionRule(rule);
			match = match && EvaluateTaxSystemRule(rule);
			match = match && EvaluateRegistrationCodeRule(rule);
			match = match && EvaluateOrganisationCategoryRule(rule);

			return match;
		}

		bool EvaluateParentTransactionRule(IAccComplianceRule rule)
		{
			var result = true;
			if (!rule.ParentTransactionSubType.IsEmpty)
			{
				var parentTransactionSubType = (EvaluateComplianceRule as IComplianceRuleParentTransaction)?.ParentTransaction?.ComplianceSubType;
				result = rule.ParentTransactionSubType == parentTransactionSubType.GetValueOrDefault();
			}

			return result;
		}
		bool EvaluateVATGroupRule(IAccComplianceRule rule)
		{
			bool result = false;
			if (rule.VATGroupRule.IsEmpty)
			{
				result = true;
			}
			else if (rule.VATGroupRule == VatGroupListCodes.VATGroupMember)
			{
				if (EvaluateComplianceRule.Header != null && EvaluateComplianceRule.Header.IsInterOfficeBillingOrgNotReportableForTax)
				{
					result = true;
				}
			}
			else if (rule.VATGroupRule == VatGroupListCodes.ExcludeVATGroupMembers)
			{
				if (EvaluateComplianceRule.Header != null && !EvaluateComplianceRule.Header.IsInterOfficeBillingOrgNotReportableForTax)
				{
					result = true;
				}
			}
			return result;
		}

		bool EvaluateTaxRegistrationLocationRule(IAccComplianceRule rule)
		{
			bool result = false;
			if (rule.TaxRegistrationLocationRule.IsEmpty)
			{
				result = true;
			}
			else if (EvaluateComplianceRule.Header != null && EvaluateComplianceRule.Header.CountryOfTaxRegistration != null && !EvaluateComplianceRule.Header.RawTaxRegistrationNumber.IsEmpty)
			{
				result = AccountingTaxLocations.GetCountryFallbackCodes(Factory, EvaluateComplianceRule.Header.CountryOfTaxRegistration, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, null, string.Empty).Select(x => ((ZString)x.ToString()).ToUpper()).Contains(rule.TaxRegistrationLocationRule.ToUpper().Trim().ToString());
			}

			return result;
		}

		bool EvaluateExporterExemptionRule(IAccComplianceRule rule)
		{
			bool? isExporterExempt()
			{
				var countryCode = GlbCompany.CurrentCompany.Country.Code;
				string ledger = EvaluateComplianceRule.Ledger;
				if (ledger == LedgerTypes.AccountsReceivable)
				{
					return EvaluateComplianceRule.Header.IsExporterExemptDebtor(countryCode);
				}

				if (ledger.In(LedgerTypes.AccountsPayable, LedgerTypes.UnapprovedPayableTransactions))
				{
					return EvaluateComplianceRule.Header.IsExporterExemptCreditor(countryCode);
				}

				return null;
			}

			var isExemptInvoice = isExporterExempt();
			if (!isExemptInvoice.HasValue)
			{
				return rule.ExporterExemption.IsEmpty;
			}

			var match = rule.ExporterExemption.IsEmpty;
			match = match || rule.ExporterExemption == ExporterExemptionCodes.Exempt && isExemptInvoice.Value;
			match = match || rule.ExporterExemption == ExporterExemptionCodes.NotExempt && !isExemptInvoice.Value;
			return match;
		}

		bool EvaluateSelfBillingRule(IAccComplianceRule rule)
		{
			return (
									rule.SelfBillingRule.IsEmpty
									|| (rule.SelfBillingRule == SelfBillingRuleCodes.SelfBillingTransactions && EvaluateComplianceRule.IsSelfBillingInvoice)
									|| (rule.SelfBillingRule == SelfBillingRuleCodes.StandardTransactions && !EvaluateComplianceRule.IsSelfBillingInvoice)
								);
		}

		bool EvaluateOrganisationLocationRule(IAccComplianceRule rule)
		{
			return (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.VietNam &&
					(rule.OrganisationLocation.IsEmpty || IsTransactionOrganizatioLocatedInsideCountry(rule.OrganisationLocation))) ||
					MeetVNOrganizationLocationRule(rule.OrganisationLocation);
		}

		[SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "Obsolete code region")]
		bool EvaluateTaxRegistrationTypeRule(IAccComplianceRule rule)
		{
			bool? result = (CountryComplianceFactory.GetIComplianceSubTypeTaxRegistrationTypeRuleProvider(rule.Country))?.IsTaxRegistrationTypeRuleApplicable(rule, EvaluateComplianceRule.Header);

			#region This code is obsolete plese dont add new countries in this section instead use CountryComplianceFactory
			if (result == null)
			{
				result = GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Peru
						 || rule.TaxRegistrationType.IsEmpty
						 || (rule.TaxRegistrationType == TaxRegistrationTypeCodes.Individual && IsTransactionOrganizationPeruIndividual)
						 || (rule.TaxRegistrationType == TaxRegistrationTypeCodes.OrgRegisteredForTaxInPeru && IsTransactionOrganizationPeruRegisteredForTax);
			}
			#endregion
			return result.Value;
		}

		bool EvaluateOriginalRule(IAccComplianceRule rule)
		{
			return (rule.OriginalRule.IsEmpty
				|| rule.OriginalRule == OriginalRuleCodes.AllTransactions
				|| (rule.OriginalRule == OriginalRuleCodes.OriginalTransactionOnly && !EvaluateComplianceRule.IsReversalTransaction && !EvaluateComplianceRule.IsAmendingTransaction)
				|| (rule.OriginalRule == OriginalRuleCodes.AmendingTransactionOnly && EvaluateComplianceRule.IsAmendingTransaction)
				|| (rule.OriginalRule == OriginalRuleCodes.ReversalTransactionOnly && EvaluateComplianceRule.IsReversalTransaction)
				|| (rule.OriginalRule == OriginalRuleCodes.AmendingReversalOnly && (EvaluateComplianceRule.IsReversalTransaction || EvaluateComplianceRule.IsAmendingTransaction))
				);
		}

		bool EvaluateDisbursementRule(IAccComplianceRule rule)
		{
			return (rule.DisbursementRule.IsEmpty
				|| rule.DisbursementRule == DisbursementRuleCodes.AllTransactions
				|| (rule.DisbursementRule == DisbursementRuleCodes.DisbursementOnly && EvaluateComplianceRule.IsDisbursementOrFinal)
				|| (rule.DisbursementRule == DisbursementRuleCodes.NonDisbursementOnly && !EvaluateComplianceRule.IsDisbursementOrFinal)
				);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool EvaluateTaxInvoiceRule(IAccComplianceRule rule)
		{
			switch (rule.TaxInvoiceRule)
			{
				case TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly:
					return AllLinesContainExcludeChargeTaxIDs;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT:
					return ContainsAtLeastOneTaxIDExcludingNOT;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL:
					return ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID:
					return IsContainsAtLeastOneTaxID;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax:
					return ContainsAtLeastOneTaxIDAndNoAmountOfTax;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax:
					return ContainsAtLeastOneTaxIDAndAmountOfTax;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs:
					return ContainsAtLeastOneTaxIDExcludingRVS;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID:
					return ContainsAtLeastOneSuspendedTaxID;
				case TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended:
					return ContainsAtLeastOneTaxIDExcludingSuspended;
				case TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly:
					return AllLinesContainReverseChargeTaxIDs;
				case TaxInvoiceRuleCodes.ContainsAnAmountOfTax:
					return ContainsAnAmountOfTax;
				case TaxInvoiceRuleCodes.ContainsNoTaxIDs:
					return !IsContainsAtLeastOneTaxID;
				case TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount:
					return AllWithTaxIDAndZeroTaxAmount;
				case TaxInvoiceRuleCodes.SpecificTaxIDs:
					return AllContainedInSpecifiedTaxIDCodes(rule.SeparatedTaxIDCodes);
				case TaxInvoiceRuleCodes.AllWithExemptTaxIDs:
					return AllWithExemptTaxIDs;
				case TaxInvoiceRuleCodes.AllWithNoReportTaxIDs:
					return AllWithNoReportTaxIDs;
				case TaxInvoiceRuleCodes.All:
				case "":
					return true;
				default:
					throw new NotSupportedException(FormattableString.Invariant($"Tax Invoice Rule '{rule.TaxInvoiceRule}' is not supported"));
			}
		}

		bool MeetVNOrganizationLocationRule(ZString organizationLocation)
		{
			bool result = false;
			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.VietNam)
			{
				if (EvaluateComplianceRule.Header != null && EvaluateComplianceRule.Header.UNLOCO != null)
				{
					var uNLOCOCountryCode = EvaluateComplianceRule.Header.UNLOCO.Country?.Code ?? ZString.Empty;
					if ((organizationLocation == Core.Constants.CountryCodes.VietNam && uNLOCOCountryCode == Core.Constants.CountryCodes.VietNam) ||
						(organizationLocation.IsEmpty && uNLOCOCountryCode != Core.Constants.CountryCodes.VietNam))
					{
						result = true;
					}
				}
			}
			return result;
		}

		#region Invoice Taxation Status

		bool IsContainsAtLeastOneTaxID
		{
			get
			{
				return LinesForReporting.Any(x => x.AL_AT.IsValid);
			}
		}

		bool ContainsAtLeastOneTaxIDAndAmountOfTax
		{
			get
			{
				return IsContainsAtLeastOneTaxID && ContainsAnAmountOfTax;
			}
		}

		bool ContainsAtLeastOneTaxIDAndNoAmountOfTax
		{
			get
			{
				return IsContainsAtLeastOneTaxID && !ContainsAnAmountOfTax;
			}
		}

		bool ContainsAtLeastOneTaxIDExcludingNOT
		{
			get
			{
				return LinesForReporting.Any(x => x.AL_AT.IsValid && x.TaxRate.AT_Type != AccTaxRate.Types.NotReportable);
			}
		}

		bool ContainsAtLeastOneTaxIDExcludingNOTAndEXL
		{
			get
			{
				return LinesForReporting.Any(x => x.AL_AT.IsValid && x.TaxRate.AT_Type != AccTaxRate.Types.NotReportable && x.TaxRate.AT_Type != AccTaxRate.Types.ExcludedFromTheTaxBase);
			}
		}

		bool ContainsAtLeastOneTaxIDExcludingRVS
		{
			get
			{
				return LinesForReporting.Any(x => x.AL_AT.IsValid && x.TaxRate.AT_Type != AccTaxRate.Types.ReverseRated);
			}
		}

		bool ContainsAtLeastOneSuspendedTaxID => LinesForReporting.Any(x => x.AL_AT.IsValid && x.TaxRate.AT_Type == AccTaxRate.Types.Suspended);

		bool ContainsAtLeastOneTaxIDExcludingSuspended => LinesForReporting.Any(x => x.AL_AT.IsValid && x.TaxRate.AT_Type != AccTaxRate.Types.Suspended);

		bool AllLinesContainReverseChargeTaxIDs
		{
			get
			{
				return LinesForReporting.All(x => x.AL_AT.IsValid && x.TaxRate.AT_Type == AccTaxRate.Types.ReverseRated);
			}
		}

		bool AllLinesContainExcludeChargeTaxIDs
		{
			get
			{
				return LinesForReporting.All(x => x.AL_AT.IsValid && x.TaxRate.AT_Type == AccTaxRate.Types.ExcludedFromTheTaxBase);
			}
		}

		bool ContainsAnAmountOfTax
		{
			get { return LinesForReporting.Sum(x => x.AL_GSTVAT) != 0; }
		}

		bool ContainsNonCommentLines
		{
			get { return LinesForReporting.Any(); }
		}

		IEnumerable<AccTransactionLines> LinesForReporting
		{
			get { return EvaluateComplianceRule.Lines.Where(x => x.ChargeCode == null || x.ChargeCode.AC_ChargeType != Constants.ChargeType.Comment); }
		}

		bool AllWithTaxIDAndZeroTaxAmount
		{
			get
			{
				var countryInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code);
				if (countryInfo is IComplianceSubTypeTaxInvoiceRuleWithTaxIDAndZeroAmount info)
				{
					return info.AllWithTaxIDAndZeroTaxAmount(LinesForReporting);
				}

				return LinesForReporting.All(x => x.AL_GSTVAT == 0 && x.AL_AT.IsValid && (x.TaxRate.AT_Type == AccTaxRate.Types.Rated || x.TaxRate.AT_Type == AccTaxRate.Types.CapitalRated || x.TaxRate.AT_Type == AccTaxRate.Types.IntegratedGST));
			}
		}

		bool AllContainedInSpecifiedTaxIDCodes(IReadOnlyCollection<ZString> splitTaxIDCodes)
		{
			return LinesForReporting.All(x => x.AL_AT.IsValid && splitTaxIDCodes.Contains(x.TaxRate.AT_Code));
		}

		bool AllWithExemptTaxIDs
		{
			get { return LinesForReporting.All(x => x.AL_AT.IsValid && x.TaxRate.AT_Type == AccTaxRate.Types.Exempt); }
		}

		bool AllWithNoReportTaxIDs
		{
			get { return LinesForReporting.All(x => x.AL_AT.IsValid && x.TaxRate.AT_Type == AccTaxRate.Types.NotReportable); }
		}

		bool IsTransactionOrganizatioLocatedInsideCountry(ZString countryCode)
		{
			return EvaluateComplianceRule.Header != null &&
				EvaluateComplianceRule.Header.UNLOCO != null &&
				AccountingTaxLocations.GetLocationFallbackCodes(Factory, EvaluateComplianceRule.Header.UNLOCO, EvaluateComplianceRule.Company.GC_RN_NKCountryCode, string.Empty)
					.Select(x => ((ZString)x.ToString()).ToUpper())
					.Contains(countryCode.ToUpper().Trim().ToString());
		}

		bool IsTransactionOrganizationPeruIndividual
		{
			get
			{
				return EvaluateComplianceRule.Header != null &&
					EvaluateComplianceRule.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.PeruCodeTypes.DNI, Core.Constants.CountryCodes.Peru) != null;
			}
		}

		bool IsTransactionOrganizationPeruRegisteredForTax
		{
			get
			{
				return EvaluateComplianceRule.Header != null &&
					EvaluateComplianceRule.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, Core.Constants.CountryCodes.Peru) != null;
			}
		}

		#endregion

		#region Tax Systems

		bool EvaluateTaxSystemRule(IAccComplianceRule rule)
		{
			var match = rule.RequiredTaxSystem.IsEmpty && rule.ExcludedTaxSystem.IsEmpty;
			if (!match)
			{
				var taxSystems = EvaluateComplianceRule.TaxTransactions.Select(x => x.ATT_TaxSystemCode).Distinct().ToHashSet();
				match = (rule.RequiredTaxSystem.IsEmpty || taxSystems.Contains(rule.RequiredTaxSystem))
					&& (rule.ExcludedTaxSystem.IsEmpty || !taxSystems.Contains(rule.ExcludedTaxSystem));
			}
			return match;
		}

		#endregion

		bool EvaluateRegistrationCodeRule(IAccComplianceRule rule)
		{
			var match = rule.RequiredRegistrationCode.IsEmpty && rule.ExcludedRegistrationCode.IsEmpty;
			if (!match)
			{
				var codes = new List<ZString>();
				if (!rule.RequiredRegistrationCode.IsEmpty)
				{
					codes.Add(rule.RequiredRegistrationCode);
				}
				if (!rule.ExcludedRegistrationCode.IsEmpty)
				{
					codes.Add(rule.ExcludedRegistrationCode);
				}

				var registrations = EvaluateComplianceRule.Header.CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(rule.Country, codes.ToArray()).Select(x => x.OK_CodeType).ToHashSet();
				match = (rule.RequiredRegistrationCode.IsEmpty || registrations.Contains(rule.RequiredRegistrationCode))
						&& (rule.ExcludedRegistrationCode.IsEmpty || !registrations.Contains(rule.ExcludedRegistrationCode));
			}
			return match;
		}

		bool EvaluateOrganisationCategoryRule(IAccComplianceRule rule)
		{
			return rule.OrganisationCategory.IsEmpty
				|| (rule.OrganisationCategory.Equals(EvaluateComplianceRule.Header?.OH_Category));
		}
	}
}
