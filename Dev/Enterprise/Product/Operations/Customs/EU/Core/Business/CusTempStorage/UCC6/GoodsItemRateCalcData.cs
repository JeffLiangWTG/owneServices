using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class GoodsItemRateCalcData : IUniversalRateCalcData
	{
		public GoodsItemRateCalcData(TemporaryStoragePackedItem goodsItem)
		{
			this.GoodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}

		protected TemporaryStoragePackedItem GoodsItem { get; }

		public DateTime DateOfValuation => GoodsItem.ValuationDate.ToDateTime();

		public decimal ValueForDuty
		{
			get
			{
				if (string.IsNullOrWhiteSpace(CustomsValueFormula))
				{
					return CustomsValue;
				}

				if (!cachedValueForDuty.HasValue)
				{
					var calcResult = new UniversalRateCalculator(CustomsValueFormula, this).Calculate();
					cachedValueForDuty = Enterprise.ZArchitecture.Core.Utilities.Round(calcResult, 3);
				}

				return cachedValueForDuty.Value;
			}
		}
		decimal? cachedValueForDuty;

		public string CustomsValueFormula
		{
			get => customsValueFormula;
			set
			{
				cachedValueForDuty = null;
				customsValueFormula = value;
			}
		}
		string customsValueFormula;

		public decimal CustomsValue => GoodsItem.API_GoodsValue;

		public IDictionary<string, decimal> UnitOfMeasureValueList => unitOfMeasureValueList ?? (unitOfMeasureValueList = GetUnitOfMeasureValueList());
		IDictionary<string, decimal> unitOfMeasureValueList;

		IDictionary<string, decimal> GetUnitOfMeasureValueList()
		{
			var dict = new Dictionary<string, decimal>();
			AddUnitOfMeasureToDict(dict, GoodsItem.API_CustomsUQ, GoodsItem.API_CustomsQty);
			AddUnitOfMeasureToDict(dict, GoodsItem.API_CustomsUQ2, GoodsItem.API_CustomsQty2);
			AddUnitOfMeasureToDict(dict, GoodsItem.API_CustomsUQ3, GoodsItem.API_CustomsQty3);

			RateCalcUnitOfMeasureAggregator.AddConvertibleUnitsToUnitOfMeasureDictionary(dict, GoodsItem.Factory, GoodsItem.API_RN_NKGoodsOrigin);

			return dict;
		}

		void AddUnitOfMeasureToDict(Dictionary<string, decimal> dict, ZString customsUnitQty, ZDecimal customsQty)
		{
			if (!customsUnitQty.IsEmpty && !customsQty.IsEmpty && !dict.ContainsKey(customsUnitQty))
			{
				dict.Add(customsUnitQty, customsQty);
			}
		}

		public IDictionary<string, decimal> CountrySpecificValueList => CountrySpecificValueListCore;

		protected IDictionary<string, decimal> CountrySpecificValueListCore => new Dictionary<string, decimal>() { { MethodOfCalculationPVP, CustomsValue } };

		public IList<Tuple<string, string>> AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();

		const string MethodOfCalculationPVP = "PVP";
	}
}
