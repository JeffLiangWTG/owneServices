using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class CusLineTariffDetailRateCalcData : IUniversalRateCalcData
{
	public CusLineTariffDetailRateCalcData(CusLineTariffDetail tariffDetail)
	{
		this.tariffDetail = Argument.NotNull(tariffDetail, nameof(tariffDetail));
	}
	readonly CusLineTariffDetail tariffDetail;

	JobComInvoiceLine InvoiceLine => tariffDetail.Parent as JobComInvoiceLine;

	public DateTime DateOfValuation => tariffDetail.EffectiveAssessmentDate.ToDateTime();

	public decimal ValueForDuty => InvoiceLine?.JI_CustomsValue ?? 0;

	public decimal CustomsValue => InvoiceLine?.JI_CustomsValue ?? 0;

	public IDictionary<string, decimal> UnitOfMeasureValueList
	{
		get
		{
			if (fUnitOfMeasureValueList == null)
			{
				fUnitOfMeasureValueList = new Dictionary<string, decimal>();
				AddToDictionaryIfNotExists(fUnitOfMeasureValueList, tariffDetail.BZ_UQ1, tariffDetail.BZ_Qty1);
			}
			return fUnitOfMeasureValueList;
		}
	}
	IDictionary<string, decimal> fUnitOfMeasureValueList;

	public IDictionary<string, decimal> CountrySpecificValueList
	{
		get
		{
			if (fCountrySpecificValueList == null)
			{
				fCountrySpecificValueList = new Dictionary<string, decimal>();
				AddToDictionaryIfNotExists(fCountrySpecificValueList, UniversalReferenceConstants.FormulaPlaceholder.DutyCalculation, InvoiceLine.JI_Calc_DutyAmount);
			}
			return fCountrySpecificValueList;
		}
	}

	IDictionary<string, decimal> fCountrySpecificValueList;

	protected void AddToDictionaryIfNotExists(IDictionary<string, decimal> keyValuePairs, ZString key, ZDecimal value)
	{
		if (!key.IsEmpty && !keyValuePairs.ContainsKey(key))
		{
			keyValuePairs.Add(key, value);
		}
	}

	public IList<Tuple<string, string>> AdditionalInformationList => null;

	public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();
}
