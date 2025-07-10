using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GoodsItemRateCalcData : IUniversalRateCalcData
	{
		public GoodsItemRateCalcData(NctsCommonCargoDesc goodsItem)
		{
			this.GoodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}

		protected NctsCommonCargoDesc GoodsItem { get; }

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
					cachedValueForDuty = Utilities.Round(calcResult, 3);
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

		public decimal CustomsValue => GoodsItem.BY_MonetaryValue;

		public IDictionary<string, decimal> UnitOfMeasureValueList => unitOfMeasureValueList ?? (unitOfMeasureValueList = GetUnitOfMeasureValueList());
		IDictionary<string, decimal> unitOfMeasureValueList;

		IDictionary<string, decimal> GetUnitOfMeasureValueList()
		{
			var dict = new Dictionary<string, decimal>();
			AddUnitOfMeasureToDict(dict, GoodsItem.BY_CustomsUnitQty, GoodsItem.BY_CustomsQuantity);
			AddUnitOfMeasureToDict(dict, GoodsItem.BY_CustomsSecondUnitQty, GoodsItem.BY_CustomsSecondQuantity);
			if (GoodsItem.IsPhase5)
			{
				AddUnitOfMeasureToDict(dict, GoodsItem.BY_CustomsThirdUnitQty, GoodsItem.BY_CustomsThirdQuantity);
				AddUnitOfMeasureToDict(dict, GoodsItem.BY_CustomsFourthUnitQty, GoodsItem.BY_CustomsFourthQuantity);
			}
			else if (GoodsItem is NctsDepartureCargoDesc departureGoodItem)
			{
				AddUnitOfMeasureToDict(dict, departureGoodItem.BY_CustomsThirdUnitQty, departureGoodItem.BY_CustomsThirdQuantity);
			}

			RateCalcUnitOfMeasureAggregator.AddConvertibleUnitsToUnitOfMeasureDictionary(dict, GoodsItem.Factory, GoodsItem.Header.CountryCode);

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

		protected virtual IDictionary<string, decimal> CountrySpecificValueListCore { get; } = new Dictionary<string, decimal>();

		public IList<Tuple<string, string>> AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();
	}
}
