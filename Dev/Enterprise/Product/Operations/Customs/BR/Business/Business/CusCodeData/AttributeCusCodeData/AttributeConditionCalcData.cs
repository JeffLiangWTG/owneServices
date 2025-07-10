using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;

namespace Enterprise.Customs.BR.Business
{
	public class AttributeConditionCalcData : IUniversalRateCalcData
	{
		public AttributeConditionCalcData(string answerDataType, ZString answer)
		{
			unitOfMeasureValueList.Add(key, answerDataType switch
			{
				AnswerDataTypes.Boolean => answer == true.ToString().ToLower() ? 1m : 0m,
				_ => decimal.TryParse(answer, out var result) ? result : 0m,
			});
		}

		readonly string key = (NoResString)"Answer";

		DateTime IUniversalRateCalcData.DateOfValuation => DateTime.MinValue;

		decimal IUniversalRateCalcData.ValueForDuty => 0m;

		decimal IUniversalRateCalcData.CustomsValue => 0m;

		IDictionary<string, decimal> IUniversalRateCalcData.UnitOfMeasureValueList => unitOfMeasureValueList;
		readonly Dictionary<string, decimal> unitOfMeasureValueList = new();

		IDictionary<string, decimal> IUniversalRateCalcData.CountrySpecificValueList => null;

		IDictionary<string, string> IUniversalRateCalcData.MeursingExpressionList => null;

		IList<Tuple<string, string>> IUniversalRateCalcData.AdditionalInformationList => null;
	}
}
