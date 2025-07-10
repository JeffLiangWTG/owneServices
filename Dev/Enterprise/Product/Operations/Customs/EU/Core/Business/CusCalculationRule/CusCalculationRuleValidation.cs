using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class CusCalculationRuleValidation : Customs.Business.CusCalculationRuleValidation
	{
		public CusCalculationRuleValidation(CusCalculationRule parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateEffectiveDates();
		}

		new CusCalculationRule Parent => (CusCalculationRule)base.Parent;

		protected override void CheckCCR_RuleType()
		{
			base.CheckCCR_RuleType();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.CCR_RuleTypeInfo);
		}

		protected override void CheckCCR_BasedOn()
		{
			base.CheckCCR_BasedOn();
			ListValidation.ErrorIfInvalidCode(Parent.CCR_BasedOnInfo);
		}

		protected override void CheckCCR_TransportMode()
		{
			base.CheckCCR_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.CCR_TransportModeInfo);
		}

		protected override void CheckCCR_RX_NKCurrency()
		{
			base.CheckCCR_RX_NKCurrency();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.CCR_RX_NKCurrencyInfo);
		}

		protected override void CheckCCR_EndDateIsNotEmpty()
		{
		}

		protected override void CheckCCR_EndDateIsValidZDateTimeOffsetRange()
		{
		}

		void ValidateEffectiveDates()
		{
			var parent = Parent;
			var startDate = parent.CCR_StartDate;
			if (!startDate.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusCalculationRule));
				query.AddToFilter(CusCalculationRuleSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
				query.AddToFilter(CusCalculationRuleSchema.CCR_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
				query.AddToFilter(CusCalculationRuleSchema.CCR_StartDate, SQLComparisonOperator.LessThanOrEqualTo, parent.CCR_EndDate);
				query.AddToFilter(CusCalculationRuleSchema.CCR_RuleType, parent.CCR_RuleType);
				query.AddToFilter(CusCalculationRuleSchema.CCR_OH_Importer, parent.CCR_OH_Importer.IsValid ? parent.CCR_OH_Importer : DBNull.Value);
				query.AddToFilter(CusCalculationRuleSchema.CCR_TransportMode, parent.CCR_TransportMode);
				query.AddToFilter(CusCalculationRuleSchema.CCR_GC_Company, parent.CCR_GC_Company);
				var anotherRuleWithOverlappingDates = parent.Factory.LoadTop1<CusCalculationRule>(query);
				if (anotherRuleWithOverlappingDates != null)
				{
					parent.AddRowError(Res.GetString("CD952B27-258A-4234-A42D-7B62AE4EC75D",
						"There's another rule with overlapping effective dates. Please enter different Start Date and/or End Date."));
				}
			}
		}

		protected override void CheckCCR_EndDate()
		{
			base.CheckCCR_EndDate();
			if (Parent.CCR_EndDate < Parent.CCR_StartDate)
			{
				Parent.CCR_EndDateInfo.AddError(Res.GetString("0AEB5B52-BE78-4027-A542-047AE6D37106", "End Date cannot be earlier than Start Date"));
			}
		}
	}
}
