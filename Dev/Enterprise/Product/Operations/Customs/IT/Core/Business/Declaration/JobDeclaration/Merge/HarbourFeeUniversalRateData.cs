using System;
using System.Collections.Generic;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class HarbourFeeUniversalRateData : IUniversalRateCalcData
{
	public HarbourFeeUniversalRateData(ZWeight grossWeight)
	{
		this.grossWeight = grossWeight;
	}

	readonly ZWeight grossWeight;

	public DateTime DateOfValuation => throw new InvalidOperationException(FormattableString.Invariant($"{HarbourFeeFormulaShouldNotRequireCaption} {nameof(DateOfValuation)}"));

	public decimal ValueForDuty => throw new InvalidOperationException(FormattableString.Invariant($"{HarbourFeeFormulaShouldNotRequireCaption} {nameof(ValueForDuty)}"));

	public decimal CustomsValue => throw new InvalidOperationException(FormattableString.Invariant($"{HarbourFeeFormulaShouldNotRequireCaption} {nameof(CustomsValue)}"));

	public IDictionary<string, decimal> UnitOfMeasureValueList => unitOfMeasureValueList ?? (unitOfMeasureValueList = GetNewUnitOfMeasureValueList());
	IDictionary<string, decimal> unitOfMeasureValueList;

	public IDictionary<string, decimal> CountrySpecificValueList { get; } = new Dictionary<string, decimal>();

	public IList<Tuple<string, string>> AdditionalInformationList { get; } = new List<Tuple<string, string>>();

	public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();

	#region Implementation

	decimal GetAndRoundGrossWeightInTon()
	{
		var grossWeightInTon = grossWeight.ConvertTo(Core.Constants.Weight.Tonnes);
		return grossWeightInTon - Math.Truncate(grossWeightInTon) > 0.1m ? Math.Ceiling(grossWeightInTon) : Math.Floor(grossWeightInTon);
	}

	Dictionary<string, decimal> GetNewUnitOfMeasureValueList()
	{
		var result = new Dictionary<string, decimal>();

		if (grossWeight.IsValid)
		{
			var grossWeightInTon = GetAndRoundGrossWeightInTon();
			result.Add(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne, grossWeightInTon);
		}

		return result;
	}

	#endregion

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
	const string HarbourFeeFormulaShouldNotRequireCaption = "Harbour Fee formula should not require the value:";
}
