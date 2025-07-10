using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	[DescriptionProperty(nameof(PermitHolderFullName))]
	public class CusGuaranteeHeader : BaseCusGuaranteeHeader, Integration.Customs.EU.ICusGuaranteeHeader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.BaseCusGuaranteeHeader.Schema
		{
		}

		public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly CusGuaranteeHeaderTypeDecider TypeDecider = new CusGuaranteeHeaderTypeDecider();

		public new CusGuaranteeHeaderValidation Validation => (CusGuaranteeHeaderValidation)base.Validation;

		public new CusGuaranteeHeaderLookups Lookups => (CusGuaranteeHeaderLookups)base.Lookups;

		protected override Customs.Business.CusPermitHeaderLookups GetNewLookups()
		{
			return new CusGuaranteeHeaderLookups(this);
		}
		protected override Customs.Business.CusPermitHeaderValidation GetNewValidation()
		{
			return new CusGuaranteeHeaderValidation(this);
		}

		public new GuaranteeCountrySpecificInstruction CountrySpecificInstruction => (GuaranteeCountrySpecificInstruction)base.CountrySpecificInstruction;

		protected override CusGuaranteeRuleCollection CreateNewCusGuaranteeRulesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this);

		protected override CusGuaranteeRuleCollection CreateNewAdditionalAccessCodesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this, PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber);

		public void AddWriteOffTransaction(ZString reference, ZDecimal writeOffTranValue, ZDateTime writeOffDate, string commentSuffix = "")
		{
			if (HasOpeningBalanceTransaction)
			{
				const string commentPrefix = "Write-off ";
				var comment = commentPrefix + (string.IsNullOrEmpty(commentSuffix) ? reference : (ZString)commentSuffix);
				AddTransaction(reference,
								comment,
								ZString.Empty,
								ZString.Empty,
								writeOffTranValue,
								ZDecimal.Zero,
								status: Customs.Business.PermitTransactionStatusList.Codes.Confirmed,
								transactionDate: writeOffDate);
			}
		}

		#region New Properties
		public bool IsPermitGuaranteeType => CPH_Type == CountrySpecificInstruction.PermitGuaranteeType;

		public ZString Description => ZString.Format("{0} {1} {2}", PermitHolder?.OH_Code ?? ZString.Empty, CPH_Type, CPH_SubType).Trim();

		public ZString PermitHolderFullName => PermitHolder?.OH_FullName ?? ZString.Empty;

		public new class Loader : BaseCusGuaranteeHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusGuaranteeHeader);
			}

			public static CusGuaranteeHeader[] LoadCusGuaranteeHeaderListFromPermitHolder(BusinessObjectFactory factory, ZGuid permitHolder, string countryCode)
			{
				CusGuaranteeHeader[] result = null;
				if (!permitHolder.IsEmpty)
				{
					var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
					query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, permitHolder);
					query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
					query.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, true);
					var dateQuery = new ZQuery();
					dateQuery.AddToFilter(CusPermitHeaderSchema.CPH_EndDate, ZDate.Empty);
					dateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
					query.AddToFilter(dateQuery);
					result = factory.Load<CusGuaranteeHeader>(query);
				}
				return result;
			}
		}

		public ZDecimal RateForDuty => CusGuaranteeRules.FirstOrDefault(rule => rule.CPR_RuleCode == PermitRuleCodeList.Codes.PCD) is { CPR_ValueFrom: var value }
		&& !string.IsNullOrWhiteSpace(value)
		? ZDecimal.Parse(value)
		: 100m;

		public ZDecimal RateForVAT => CusGuaranteeRules.FirstOrDefault(rule => rule.CPR_RuleCode == PermitRuleCodeList.Codes.PCV) is { CPR_ValueFrom: var value }
		&& !string.IsNullOrWhiteSpace(value)
		? ZDecimal.Parse(value)
		: 100m;

		public ZDecimal RateForOtherFees => CusGuaranteeRules.FirstOrDefault(rule => rule.CPR_RuleCode == PermitRuleCodeList.Codes.PCP) is { CPR_ValueFrom: var value }
		&& !string.IsNullOrWhiteSpace(value)
		? ZDecimal.Parse(value)
		: 100m;
		#endregion
	}
}

