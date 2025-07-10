using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class ConditionCalcDataForGuidedDecisionMakingBasic : IUniversalRateCalcData
	{
		public ConditionCalcDataForGuidedDecisionMakingBasic(GuidedDecisionMakingBasic guidedDecisionMakingBasic)
		{
			GDMBasic = Argument.NotNull(guidedDecisionMakingBasic, nameof(guidedDecisionMakingBasic));
		}
		protected readonly GuidedDecisionMakingBasic GDMBasic;

		public DateTime DateOfValuation => GDMBasic.EffectiveDate.ToDateTime();

		public decimal ValueForDuty => 0m;

		public decimal CustomsValue => 0m;

		public IDictionary<string, decimal> UnitOfMeasureValueList
		{
			get
			{
				if (fUnitOfMeasureValueList == null)
				{
					fUnitOfMeasureValueList = new Dictionary<string, decimal>();
					AddToDictionaryIfNotExists(fUnitOfMeasureValueList, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, GDMBasic.CustomsFirstQuantity);
					AddToDictionaryIfNotExists(fUnitOfMeasureValueList, GDMBasic.CustomsSecondUnitQty, GDMBasic.CustomsSecondQuantity);
					AddToDictionaryIfNotExists(fUnitOfMeasureValueList, GDMBasic.CustomsThirdUnitQty, GDMBasic.CustomsThirdQuantity);
				}
				return fUnitOfMeasureValueList;
			}
		}
		IDictionary<string, decimal> fUnitOfMeasureValueList;

		public IDictionary<string, decimal> CountrySpecificValueList => null;

		public IList<Tuple<string, string>> AdditionalInformationList => null;

		public IDictionary<string, string> MeursingExpressionList => null;

		protected void AddToDictionaryIfNotExists(IDictionary<string, decimal> keyValuePairs, ZString key, ZDecimal value)
		{
			if (!key.IsEmpty && !keyValuePairs.ContainsKey(key))
			{
				keyValuePairs.Add(key, value);
			}
		}
	}
}
