using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecordCalculator
	{
		void CalculateTaxRecords(ITaxRecordParent taxParent, List<AccTaxTransaction> allTaxRecords);
		void SetDatesFromPostDateIfApplicable(AccTaxTransaction taxTransaction, ITaxRecordParent taxParent);
		IReadOnlyCollection<(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot)> GetTaxRecordsWithLinePivots(ITaxRecordParentBase taxParent, IEnumerable<ZString> taxSystemCodesForFilter = null);
	}

	class TaxRecordCalculator : ITaxRecordCalculator
	{
		public TaxRecordCalculator(ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper)
		{
			taxRecordPivotProcessor_constructorInitializedOnly = new TaxRecordPivotProcessor();
			TaxFrameworkConfigurationHelper = Argument.NotNull(taxFrameworkConfigurationHelper, nameof(taxFrameworkConfigurationHelper));
		}

		ITaxFrameworkConfigurationHelper TaxFrameworkConfigurationHelper { get; }

		ITaxRecordPivotProcessor TaxRecordPivotProcessor => taxRecordPivotProcessor_constructorInitializedOnly;
		ITaxRecordPivotProcessor taxRecordPivotProcessor_constructorInitializedOnly;

#if DEBUG

		public void SubstituteTaxRecordPivotProcessor_ForTestOnly(ITaxRecordPivotProcessor replacement) => taxRecordPivotProcessor_constructorInitializedOnly = replacement;
		public ITaxRecordPivotProcessor TaxRecordPivotProcessor_ExposedForTestOnly => TaxRecordPivotProcessor;

#endif

		void ITaxRecordCalculator.CalculateTaxRecords(ITaxRecordParent taxParent, List<AccTaxTransaction> allTaxRecords)
		{
			var lines = taxParent.GetLines();
			var newOrUpdatedTaxRecords = CalculateTaxRecordsWithoutAmounts(taxParent, allTaxRecords, lines);
			SetTaxAmounts(newOrUpdatedTaxRecords.Select(x => x.taxRecord).ToHashSet(), TaxFrameworkConfigurationHelper);
		}

		void ITaxRecordCalculator.SetDatesFromPostDateIfApplicable(AccTaxTransaction taxTransaction, ITaxRecordParent taxParent) => SetDatesFromPostDateIfApplicable(taxTransaction, taxParent);

		IReadOnlyCollection<(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot)> ITaxRecordCalculator.GetTaxRecordsWithLinePivots(ITaxRecordParentBase taxParent, IEnumerable<ZString> taxSystemCodesForFilter)
		{
			return CalculateTaxRecordsWithoutAmounts(taxParent, new List<AccTaxTransaction>(), taxParent.GetLines(), taxSystemCodesForFilter);
		}

		IReadOnlyCollection<(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot)> CalculateTaxRecordsWithoutAmounts(ITaxRecordParentBase taxParent, List<AccTaxTransaction> allTaxRecords, IReadOnlyCollection<ITaxableTransactionLineBase> linesToCalculateTaxFor, IEnumerable<ZString> taxSystemCodesForFilter = null)
		{
			var newOrUpdatedTaxRecords = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			var lineBranches = linesToCalculateTaxFor.Where(x => x.Branch != null).Select(x => x.Branch).Distinct().ToArray();
			if (lineBranches.Any())
			{
				var invoiceTaxOverrideGroupPKs = GetInvoiceTaxOverrideGroupPKs(taxParent.Factory, taxParent.Ledger, taxParent.Org, lineBranches, taxSystemCodesForFilter);
				if (invoiceTaxOverrideGroupPKs.TaxOverrideGroupPKs.Any())
				{
					foreach (var line in linesToCalculateTaxFor)
					{
						var lineTaxOverrideGroupPKs = GetLineTaxOverrideGroupPKs(taxParent.Factory, line.ChargeCode.PK, line.Branch, invoiceTaxOverrideGroupPKs.TaxOverrideGroupPKs);
						var lineTaxRules = lineTaxOverrideGroupPKs.Select(x => GetLineTaxRule(x, line)).Where(x => x != null).ToArray();
						var newOrUpdatedTaxRecordsForLine = SetupTaxRecordsForLine(allTaxRecords, line, lineTaxRules, taxParent, invoiceTaxOverrideGroupPKs.TaxOverrideGroupInfos);
						newOrUpdatedTaxRecords.AddRange(newOrUpdatedTaxRecordsForLine);
					}
				}
			}

			return newOrUpdatedTaxRecords.ToHashSet();
		}

		static void SetTaxAmounts(IReadOnlyCollection<AccTaxTransaction> taxRecords, ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper)
		{
			foreach (var taxRecord in taxRecords)
			{
				using (taxRecord.GetValidationSuspender())
				{
					CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper);
				}
			}
		}

		static (Dictionary<ZGuid, ZGuid[]> TaxOverrideGroupPKs, Dictionary<ZGuid, TaxOverrideGroupInfo[]> TaxOverrideGroupInfos) GetInvoiceTaxOverrideGroupPKs(BusinessObjectFactory factory, ZString ledger, OrgHeader organization, GlbBranch[] branches, IEnumerable<ZString> taxSystemCodesForFilter = null)
		{
			var companyPKfromBranches = branches.First().GB_GC;

			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@Ledger", ledger, AccTaxConfigurationSchema.ETC_Ledger),
				ZSqlParameter.New("@OrgPK", organization.PK, OrgCompanyDataSchema.PK),
				ZSqlParameter.New("@CompanyPK", companyPKfromBranches, OrgCompanyDataSchema.OB_GC),
				ZSqlParameter.New("@CreationTrigger", TaxRecordCreationTrigger.PostDate.Code, AccTaxConfigurationSchema.ETC_TaxRecordCreationTrigger),
			};

			var branchPKs = new ZStringBuilder();
			for (int i = 0; i < branches.Length; i++)
			{
				var parameterName = Invariant($"@Branch{i + 1}PK");
				parameters.Add(ZSqlParameter.New(parameterName, branches[i].PK, AccTaxConfigurationSchema.ETC_ParentId));
				branchPKs.Append(parameterName);
			}

			var sqlText = $@"
SELECT 
	ETC_ParentId, 
	AX_PK,
	AXP_TaxAuthorityServiceCode,
	AXP_TaxAuthorityServiceCodeDescription,
	AXP_RateNumerator,
	AXP_RateDenominator,
	AXP_AX_TaxOverrideGroup,
	AXP_ETC_TaxConfiguration,
	AXP_AT_TaxID,
	AXP_A9_DefaultVATClass
FROM 
	dbo.AccTaxConfiguration
	INNER JOIN dbo.AccOrgTaxConfiguration ON ETC_PK = OTC_ETC
	INNER JOIN dbo.OrgCompanyData ON OTC_OB = OB_PK
	INNER JOIN dbo.AccTaxOverrideGroupTaxConfigurationPivot ON AXP_ETC_TaxConfiguration = ETC_PK
	INNER JOIN dbo.AccTaxOverrideGroup ON AXP_AX_TaxOverrideGroup = AX_PK
WHERE 
	ETC_ParentId IN (@CompanyPK, {branchPKs.ToStringWithDelimiterBetweenAppends(", ")})
	AND OB_OH = @OrgPK
	AND OB_GC = @CompanyPK
	AND ETC_IsActive = 1
	AND OTC_IsActive = 1
	AND ETC_Ledger = @Ledger
	AND ETC_TaxRecordCreationTrigger  = @CreationTrigger
";

			if (taxSystemCodesForFilter != null && taxSystemCodesForFilter.Any())
			{
				var taxSystemCodesBuilder = new ZStringBuilder();
				var taxSystemsArray = taxSystemCodesForFilter.ToArray();
				for (int i = 0; i < taxSystemsArray.Length; i++)
				{
					var parameterName = Invariant($"@TaxSystemCode{i + 1}");
					parameters.Add(ZSqlParameter.New(parameterName, taxSystemsArray[i], AccTaxConfigurationSchema.ETC_TaxSystemCode));
					taxSystemCodesBuilder.Append(parameterName);
				}

				sqlText += $@" AND ETC_TaxSystemCode IN ({taxSystemCodesBuilder.ToStringWithDelimiterBetweenAppends(", ")})";
			}

			var taxOverideGroupInfos = new DynamicBusinessObjectCollection(factory);
			taxOverideGroupInfos.Load(sqlText, parameters);

			var taxOverrideGroupPKs = (from taxGroupInfo in taxOverideGroupInfos
									   group taxGroupInfo by (ZGuid)taxGroupInfo[AccTaxConfigurationSchema.ETC_ParentId] into taxGroupInfoPerParentId
									   select taxGroupInfoPerParentId)
										.ToDictionary(taxGroupInfoPerParentId => taxGroupInfoPerParentId.Key, taxGroupInfoPerParentId => taxGroupInfoPerParentId.Select(t => (ZGuid)t[AccTaxOverrideGroupSchema.PK]).ToArray());
			var taxOverrideGroupInfos = (from taxGroupInfo in taxOverideGroupInfos
										 group taxGroupInfo by (ZGuid)taxGroupInfo[AccTaxOverrideGroupSchema.PK] into taxGroupInfoPerPK
										 select taxGroupInfoPerPK)
										.ToDictionary(taxGroupInfoPerPK => taxGroupInfoPerPK.Key, taxGroupInfoPerPK => taxGroupInfoPerPK.Select(taxGroupInfo => GetTaxOverrideGroupInfo(taxGroupInfo)).ToArray());

			return (taxOverrideGroupPKs, taxOverrideGroupInfos);

			TaxOverrideGroupInfo GetTaxOverrideGroupInfo(DynamicBusinessObject taxGroupInfo)
			{
				return new TaxOverrideGroupInfo
				(
					(ZString)taxGroupInfo[AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_TaxAuthorityServiceCode],
					(ZString)taxGroupInfo[AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_TaxAuthorityServiceCodeDescription],
					(ZInt)taxGroupInfo[AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_RateNumerator],
					(ZInt)taxGroupInfo[AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_RateDenominator],
					(ZGuid)taxGroupInfo[AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_ETC_TaxConfiguration],
					(ZGuid)taxGroupInfo[AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AT_TaxID],
					(ZGuid)taxGroupInfo[AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_A9_DefaultVATClass]
				);
			}
		}

		static ZGuid[] GetLineTaxOverrideGroupPKs(BusinessObjectFactory factory, ZGuid chargeCodePK, GlbBranch branch, Dictionary<ZGuid, ZGuid[]> invoiceTaxOverrideGroupPKs)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@ChargeCode", chargeCodePK, AccChargeCodeSchema.PK)
			};

			var taxOverrides = new ZStringBuilder();
			if (invoiceTaxOverrideGroupPKs.TryGetValue(branch.PK, out var taxOverridesOfBranch))
			{
				for (int i = 0; i < taxOverridesOfBranch.Length; i++)
				{
					var parameterName = Invariant($"@TaxOverrideBranch{i + 1}PK");
					parameters.Add(ZSqlParameter.New(parameterName, taxOverridesOfBranch[i], GlbBranchSchema.PK));
					taxOverrides.Append(parameterName);
				}
			}

			if (invoiceTaxOverrideGroupPKs.TryGetValue(branch.Company.PK, out var taxOverridesOfCompany))
			{
				for (int i = 0; i < taxOverridesOfCompany.Length; i++)
				{
					var parameterName = Invariant($"@TaxOverrideCompany{i + 1}PK");
					parameters.Add(ZSqlParameter.New(parameterName, taxOverridesOfCompany[i], GlbCompanySchema.PK));
					taxOverrides.Append(parameterName);
				}
			}

			if (taxOverrides.Length > 0)
			{
				var sqlText = $@"
SELECT 
	ACP_AX_TaxOverrideGroup
FROM 
	dbo.AccTaxOverrideGroupChargeCodePivot
WHERE 
	ACP_AC_ChargeCode = @ChargeCode
	AND ACP_AX_TaxOverrideGroup IN ({taxOverrides.ToStringWithDelimiterBetweenAppends(", ")})";

				var collection = new DynamicBusinessObjectCollection(factory);
				collection.Load(sqlText, parameters);

				return (collection.Select(x => (ZGuid)x[AccTaxOverrideGroupChargeCodePivotSchema.ACP_AX_TaxOverrideGroup]).ToArray());
			}
			else
			{
				return System.Array.Empty<ZGuid>();
			}
		}

		static AccChargeTaxOverride GetLineTaxRule(ZGuid invoiceTaxOverrideGroupPK, ITaxableTransactionLineBase line)
		{
			var taxOverrideGroup = line.Factory.Load<AccTaxOverrideGroup>(invoiceTaxOverrideGroupPK);

			return taxOverrideGroup?.GetChargeTaxOverride(line.GetTaxCalculationParameters());
		}

		static AccTaxOverrideGroup GetTaxOverrideGroup(AccChargeTaxOverride taxRule) => taxRule.TaxOverrideGroup;

		static (ZInt numerator, ZInt denominator)? GetTaxRate(AccTaxRate taxID, ZInt taxOverrideGroupRateNumerator, ZInt taxOverrideGroupRateDenominator, AccTaxConfiguration taxConfiguration, OrgHeader organization, ZDate taxDate)
		{
			if (taxID.AT_RateSource == TaxRateSources.OrganisationOnly.Code)
			{
				return organization.CompanyData.GetTaxRate(taxConfiguration, taxDate);
			}
			else if (taxID.AT_RateSource == TaxRateSources.TaxGroupOnly.Code)
			{
				return (taxOverrideGroupRateNumerator, taxOverrideGroupRateDenominator);
			}
			else if (taxID.AT_RateSource == TaxRateSources.TaxIDOnly.Code)
			{
				return taxID.GetTaxRate(taxDate);
			}
			else if (taxID.AT_RateSource == TaxRateSources.OrganisationFallbackToTaxGroup.Code)
			{
				var value = organization.CompanyData.GetTaxRate(taxConfiguration, taxDate);
				if (value == null)
				{
					return (taxOverrideGroupRateNumerator, taxOverrideGroupRateDenominator);
				}
				return value;
			}
			else if (taxID.AT_RateSource == TaxRateSources.OrganisationFallbackToTaxID.Code)
			{
				var value = organization.CompanyData.GetTaxRate(taxConfiguration, taxDate);
				if (value == null)
				{
					return taxID.GetTaxRate(taxDate);
				}
				return value;
			}
			else
			{
				throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("ECAD593F-6EC9-471D-BA31-0BDA548B90C5", "{0} rate source was not found.", taxID.AT_RateSource));
			}
		}

		(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot) CalculateTaxRecord(List<AccTaxTransaction> allTaxRecords, ITaxableTransactionLineBase line, AccTaxRate taxId, AccTaxConfiguration taxConfiguration,
			(ZInt numerator, ZInt denominator)? taxRate, ITaxRecordParentBase taxParent, ZGuid taxMessagePK, ZString taxAuthorityServiceCode, ZString taxAuthorityServiceCodeDescription)
		{
			if (taxRate.HasValue && taxRate.Value.numerator == 0)
			{
				return (null, null);
			}

			var rateNumerator = taxRate?.numerator ?? 0;
			var rateDenominator = taxRate?.denominator ?? 1;

			var taxTransaction = allTaxRecords.FirstOrDefault(x => x.ATT_AT_TaxID == taxId.PK && x.ATT_ETC == taxConfiguration.PK && x.ATT_RateNumerator == rateNumerator && x.ATT_RateDenominator == rateDenominator && x.ATT_A9_TaxMessage == taxMessagePK && x.ATT_TaxAuthorityServiceCode == taxAuthorityServiceCode);
			if (taxTransaction == null)
			{
				var taxSystem = TaxFrameworkConfigurationHelper.GetTaxSystemForCalculation(taxConfiguration.ETC_TaxSystemCode, taxConfiguration.Factory);

				taxTransaction = line.Factory.New<AccTaxTransaction>();
				allTaxRecords.Add(taxTransaction);

				using (taxTransaction.GetValidationSuspender())
				{
					taxTransaction.ATT_AH = taxParent.PK;
					taxTransaction.ATT_GC = taxParent.Company.PK;
					taxTransaction.ATT_GB = taxConfiguration.ETC_ParentTableCode == GlbCompanySchema.Constants.Prefix ? taxParent.Branch.PK : line.Branch.PK;
					taxTransaction.ATT_GE_Department = taxParent.Department.PK;
					taxTransaction.ATT_Ledger = taxConfiguration.ETC_Ledger;
					taxTransaction.ATT_ETC = taxConfiguration.PK;
					taxTransaction.ATT_TaxSystemCode = taxConfiguration.ETC_TaxSystemCode;
					SetBasis();
					SetDatesFromPostDateIfApplicable(taxTransaction, taxParent);
					taxTransaction.ATT_RX_NKOSTaxCurrency = taxParent.Currency;
					taxTransaction.ATT_AT_TaxID = taxId.PK;
					taxTransaction.ATT_TaxDate = line.TaxDate;
					taxTransaction.ATT_A9_TaxMessage = taxMessagePK;
					taxTransaction.ATT_RateNumerator = rateNumerator;
					taxTransaction.ATT_RateDenominator = rateDenominator;
					taxTransaction.ATT_AffectsSourceTransactionTotal = taxSystem.IncludeInInvoceTotal;
					taxTransaction.ATT_TaxSuperType = taxSystem.TaxSuperType;
					taxTransaction.ATT_TaxAuthorityServiceCode = taxAuthorityServiceCode;
					taxTransaction.ATT_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription;
					taxTransaction.ATT_AG_LedgerControlAccount = taxConfiguration.ETC_AG_LedgerControlAccount;
					taxTransaction.ATT_AG_TaxControlAccount = taxConfiguration.ETC_AG_TaxControlAccount;
					taxTransaction.ATT_AG_TaxExpenseAccount = taxConfiguration.ETC_AG_TaxExpenseAccount;
					taxTransaction.ATT_AG_TaxPendingControlAccount = taxConfiguration.ETC_AG_TaxPendingControlAccount;
				}
			}
			else
			{
				if (taxTransaction.ATT_TaxDate > line.TaxDate)
				{
					taxTransaction.ATT_TaxDate = line.TaxDate;
				}
			}

			var linePivot = TaxRecordPivotProcessor.Create(taxTransaction, line);

			return (taxTransaction, linePivot);

			void SetBasis()
			{
				if (taxConfiguration.ETC_TaxRealisationMethod == TaxRealisationMethods.PostDate.Code)
				{
					taxTransaction.ATT_Basis = TaxBasisList.Posting.Code;
				}
				else if (taxConfiguration.ETC_TaxRealisationMethod == TaxRealisationMethods.MatchDate.Code)
				{
					taxTransaction.ATT_Basis = TaxBasisList.Matching.Code;
				}
				else if (taxConfiguration.ETC_TaxRealisationMethod == TaxRealisationMethods.PostDateOfMatchTransaction.Code)
				{
					taxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
				}
				else
				{
					throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("CCD8B720-38D9-466A-96B9-E994444DAC1C", "Tax configuration '{0}' has not supported realization method '{1}'.", taxConfiguration.ETC_Code, taxConfiguration.ETC_TaxRealisationMethod));
				}
			}
		}

		static void SetDatesFromPostDateIfApplicable(AccTaxTransaction taxTransaction, ITaxRecordParentBase taxParent)
		{
			taxTransaction.ATT_PostDate = taxParent.PostDate.Date;
			if (taxTransaction.ATT_Basis == TaxBasisList.Posting.Code)
			{
				taxTransaction.ATT_RealisationDate = taxTransaction.ATT_PostDate;
			}
		}

		static void CalculateTaxRecordAmounts(AccTaxTransaction taxRecord, ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper)
		{
			var taxSystemsConfiguration = taxFrameworkConfigurationHelper.GetTaxSystemForCalculation(taxRecord.ATT_TaxSystemCode, taxRecord.Factory);
			CalculateTaxBaseAmounts(taxRecord, taxSystemsConfiguration);

			CalculateTaxAmounts(taxRecord, taxSystemsConfiguration);
		}

		static void CalculateTaxBaseAmounts(AccTaxTransaction taxRecord, TaxSystemsConfiguration taxSystemsConfiguration)
		{
			ZDecimal osTaxBaseAmount = 0m;
			ZDecimal localTaxBaseAmount = 0m;
			var isTaxRecordCurrencyLocal = taxRecord.IsOSTaxCurrencyLocal;
			if (taxSystemsConfiguration.TaxBaseCalculationMethod == TaxBaseCalculationMethods.InvoiceLineAmount.Code)
			{
				var pivots = taxRecord.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK));
				foreach (var pivot in pivots)
				{
					osTaxBaseAmount += pivot.BaseOSAmount;
					localTaxBaseAmount += pivot.LocalTaxBaseAmount;
				}
			}
			else
			{
				throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("7E293DBA-AE13-4FBA-9729-163D2CC97B0F", "Tax Base Calculation Method {0} for the Tax System is not supported.", taxSystemsConfiguration.TaxBaseCalculationMethod));
			}

			taxRecord.ATT_OSTaxBaseAmount = osTaxBaseAmount;
			taxRecord.ATT_LocalTaxBaseAmount = localTaxBaseAmount;
		}

		static void CalculateTaxAmounts(AccTaxTransaction taxRecord, TaxSystemsConfiguration taxSystemsConfiguration)
		{
			ZDecimal rate;
			if (taxSystemsConfiguration.TaxAmountCalculationMethod == TaxAmountCalculationMethods.BaseTimesRate.Code)
			{
				rate = new ZDecimal(taxRecord.ATT_RateNumerator) / taxRecord.ATT_RateDenominator / 100;
			}
			else
			{
				throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("3279E038-DD89-4038-A15C-F0E29498F99C", "Tax Amount Calculation Method {0} for the Tax System is not supported.", taxSystemsConfiguration.TaxAmountCalculationMethod));
			}

			int sign;
			var taxSystemAdjustmentSign = taxSystemsConfiguration.AdjustmentSign;
			if (taxSystemAdjustmentSign == TaxCalculationAdjustmentSigns.Negative.Code)
			{
				sign = -1;
			}
			else if (taxSystemAdjustmentSign == TaxCalculationAdjustmentSigns.Positive.Code)
			{
				sign = 1;
			}
			else
			{
				throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("F25B1D25-7B20-4EED-B42B-F356F397972D", "Tax System Adjustment Sign option {0} is not supported.", taxSystemAdjustmentSign));
			}

			taxRecord.SetEffectiveRateOnTaxRecordCreation(rate * sign);

			taxRecord.CalculateTaxAmountFromTaxBaseAmounts();
		}

		IReadOnlyCollection<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)> SetupTaxRecordsForLine(List<AccTaxTransaction> allTaxRecords, ITaxableTransactionLineBase line, AccChargeTaxOverride[] taxRules, ITaxRecordParentBase taxParent,
			Dictionary<ZGuid, TaxOverrideGroupInfo[]> taxGroupInfoByTaxGroupPK)
		{
			var newOrUpdatedTaxRecords = new List<(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot)>();

			foreach (var taxRule in taxRules)
			{
				var taxOverrideGroup = GetTaxOverrideGroup(taxRule) ?? throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("71693FF2-D9F0-48D4-B7BA-A5EF4C5B6895", "Tax Group is not found for a Tax Override."));

				foreach (var taxGroupInfo in taxGroupInfoByTaxGroupPK[taxOverrideGroup.PK])
				{
					var useTaxGroupInfoTax = !taxGroupInfo.TaxIDPK.IsEmpty;
					var taxId = useTaxGroupInfoTax ? line.Factory.Load<AccTaxRate>(taxGroupInfo.TaxIDPK) : taxRule.TaxRate ?? throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("759329E7-AD46-4A31-9B97-BD4C232B226A", "Tax ID is not found for a Tax Override from Tax Group with code {0}.", taxOverrideGroup.AX_Code));

					var defaultVATClass = useTaxGroupInfoTax ? taxGroupInfo.DefaultVATClassPK : taxRule.AO_A9_DefaultVATClass;

					var taxConfiguration = line.Factory.Load<AccTaxConfiguration>(taxGroupInfo.TaxConfigurationPK) ?? throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("49AD50B7-20AC-408D-BFFE-8482BCBB5A29", "Tax Configuration is not found for a Tax Group with code {0}.", taxOverrideGroup.AX_Code));

					var taxRate = GetTaxRate(taxId, taxGroupInfo.RateNumerator, taxGroupInfo.RateDenominator, taxConfiguration, taxParent.Org, line.TaxDate);

					if (taxParent.Company == null)
					{
						throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("2C5B1B49-84EE-447C-A3BF-C0819D41C683", "Transaction company is not found."));
					}
					var newOrUpdatedTaxRecord = CalculateTaxRecord(allTaxRecords, line, taxId, taxConfiguration, taxRate, taxParent, defaultVATClass, taxGroupInfo.TaxAuthorityServiceCode, taxGroupInfo.TaxAuthorityServiceCodeDescription);
					if (newOrUpdatedTaxRecord.taxRecord != null)
					{
						if (newOrUpdatedTaxRecords.Any(x => x.linePivot.ATP_ATT == newOrUpdatedTaxRecord.linePivot.ATP_ATT && x.linePivot.ATP_AL_TransactionLine == newOrUpdatedTaxRecord.linePivot.ATP_AL_TransactionLine))
						{
							throw new TaxFrameworkConfigurationValueException(ResString.GetMultilingualString("cd8e1644-a7e9-4c1d-9601-f0e3a39c4f3d", "Please review the Tax Override Group defaulting rules for Tax configuration '{0}' and Charge Code '{1}'. Taxes cannot be calculated correctly because duplicate tax defaulting rules exist for Charge Code '{1}' across more than one Tax Override Group rule set.", taxConfiguration.ETC_Code, line.ChargeCode.AC_Code));
						}
						newOrUpdatedTaxRecords.Add(newOrUpdatedTaxRecord);
					}
				}
			}

			return newOrUpdatedTaxRecords.ToHashSet();
		}

		internal class TaxOverrideGroupInfo
		{
			public TaxOverrideGroupInfo(
						ZString taxAuthorityServiceCode,
						ZString taxAuthorityServiceCodeDescription,
						ZInt rateNumerator,
						ZInt rateDenominator,
						ZGuid taxConfigurationPK,
						ZGuid taxIDPK,
						ZGuid defaultVATClassPK)
			{
				TaxAuthorityServiceCode = taxAuthorityServiceCode;
				TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription;
				RateNumerator = rateNumerator;
				RateDenominator = rateDenominator;
				TaxConfigurationPK = taxConfigurationPK;
				TaxIDPK = taxIDPK;
				DefaultVATClassPK = defaultVATClassPK;
			}

			public ZString TaxAuthorityServiceCode { get; }
			public ZString TaxAuthorityServiceCodeDescription { get; }
			public ZInt RateNumerator { get; }
			public ZInt RateDenominator { get; }
			public ZGuid TaxConfigurationPK { get; }
			public ZGuid TaxIDPK { get; }
			public ZGuid DefaultVATClassPK { get; }
		}

#if DEBUG
		internal static class TaxRecordCalculator_ForTestsOnly
		{
			public static (Dictionary<ZGuid, ZGuid[]> TaxOverrideGroupPKs, Dictionary<ZGuid, TaxOverrideGroupInfo[]> TaxOverrideGroupInfos) GetInvoiceTaxOverrideGroupPKs(BusinessObjectFactory factory, ZString ledger, OrgHeader organization, GlbBranch[] branches) => TaxRecordCalculator.GetInvoiceTaxOverrideGroupPKs(factory, ledger, organization, branches);

			public static ZGuid[] GetLineTaxOverrideGroupPKs(BusinessObjectFactory factory, ZGuid chargeCodePK, GlbBranch branch, Dictionary<ZGuid, ZGuid[]> invoiceTaxOverrideGroupPKs) => TaxRecordCalculator.GetLineTaxOverrideGroupPKs(factory, chargeCodePK, branch, invoiceTaxOverrideGroupPKs);

			public static AccChargeTaxOverride GetLineTaxRule(ZGuid invoiceTaxOverrideGroupPK, ITaxableTransactionLine line) => TaxRecordCalculator.GetLineTaxRule(invoiceTaxOverrideGroupPK, line);

			public static AccTaxOverrideGroup GetTaxOverrideGroup(AccChargeTaxOverride taxRule) => TaxRecordCalculator.GetTaxOverrideGroup(taxRule);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "For test only")]
			public static (ZInt numerator, ZInt denominator)? GetTaxRate(AccTaxRate taxID, ZInt taxOverrideGroupRateNumerator, ZInt taxOverrideGroupRateDenominator, AccTaxConfiguration taxConfiguration, OrgHeader organization, ZDate taxDate) => TaxRecordCalculator.GetTaxRate(taxID, taxOverrideGroupRateNumerator, taxOverrideGroupRateDenominator, taxConfiguration, organization, taxDate);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "For test only")]
			public static (AccTaxTransaction, AccTaxRecordTransactionLinePivot) CalculateTaxRecord(List<AccTaxTransaction> allTaxRecords, ITaxableTransactionLine line, AccTaxRate taxId, AccTaxConfiguration taxConfiguration, (ZInt numerator, ZInt denominator)? taxRate, ITaxRecordParent taxParent, ZGuid taxMessagePK, ZString taxAuthorityServiceCode, ZString taxAuthorityServiceCodeDescription) => new TaxRecordCalculator(new TaxFrameworkConfigurationHelper()).CalculateTaxRecord(allTaxRecords, line, taxId, taxConfiguration, taxRate, taxParent, taxMessagePK, taxAuthorityServiceCode, taxAuthorityServiceCodeDescription);

			public static void CalculateTaxRecordAmounts(AccTaxTransaction taxRecord, ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper) => TaxRecordCalculator.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper);

			public static IReadOnlyCollection<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)> SetupTaxRecordsForLine(List<AccTaxTransaction> allTaxRecords, ITaxableTransactionLine line, AccChargeTaxOverride[] taxRules, ITaxRecordParent taxParent, Dictionary<ZGuid, TaxOverrideGroupInfo[]> taxGroupInfoByTaxGroupPK) => new TaxRecordCalculator(new TaxFrameworkConfigurationHelper()).SetupTaxRecordsForLine(allTaxRecords, line, taxRules, taxParent, taxGroupInfoByTaxGroupPK);

			public static void SetTaxAmounts(AccTaxTransaction[] taxRecords, ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper) => TaxRecordCalculator.SetTaxAmounts(taxRecords, taxFrameworkConfigurationHelper);
		}
#endif
	}
}
