using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class REAUniversalRateCalcData : IUniversalRateCalcData
	{
		public REAUniversalRateCalcData(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		public DateTime DateOfValuation => invoiceLine.EffectiveAssessmentDate.ToDateTime();

		public decimal ValueForDuty => (cachedValueForDuty ?? (cachedValueForDuty = GetValueForDuty())).Value;
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

		public decimal CustomsValue => invoiceLine.CusEntryLine.CL_CustomsValue;

		public IDictionary<string, decimal> UnitOfMeasureValueList => fUnitOfMeasureValueList ?? (fUnitOfMeasureValueList = GetNewUnitOfMeasureValueList());
		IDictionary<string, decimal> fUnitOfMeasureValueList;

		public IDictionary<string, decimal> CountrySpecificValueList { get; } = new Dictionary<string, decimal>();

		IList<Tuple<string, string>> IUniversalRateCalcData.AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		IDictionary<string, string> IUniversalRateCalcData.MeursingExpressionList { get; } = new Dictionary<string, string>();

		decimal GetValueForDuty()
		{
			decimal valueForDuty = CustomsValue;
			if (!string.IsNullOrEmpty(CustomsValueFormula))
			{
				var calcResult = new UniversalRateCalculator(CustomsValueFormula, this).Calculate();
				valueForDuty = Utilities.Round(calcResult, 3);
			}
			return valueForDuty;
		}

		IDictionary<string, decimal> GetNewUnitOfMeasureValueList()
		{
			var unitOfMeasureValueList = new Dictionary<string, decimal>();

			AddUnitAndItsConversions(unitOfMeasureValueList, invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty);
			AddUnitAndItsConversions(unitOfMeasureValueList, invoiceLine.JI_CustomsSecondQuantity, invoiceLine.JI_CustomsSecondUnitQty);
			AddUnitAndItsConversions(unitOfMeasureValueList, invoiceLine.JI_CustomsThirdQuantity, invoiceLine.JI_CustomsThirdUnitQty);
			AddUnitAndItsConversions(unitOfMeasureValueList, invoiceLine.JI_CustomsFourthQuantity, invoiceLine.JI_CustomsFourthUnitQty);

			return unitOfMeasureValueList;
		}

		void AddUnitAndItsConversions(Dictionary<string, decimal> list, decimal currentQuantity, string currentQuantityUQ)
		{
			if (weightConversionDictionary.ContainsKey(currentQuantityUQ))
			{
				weightConversionDictionary.ForEach(conversion => list.AddIfNotExists(conversion.Key, ConvertQuantity(currentQuantity, currentQuantityUQ, conversion.Key, weightConversionDictionary)));
			}
			else if (volumeConversionDictionary.ContainsKey(currentQuantityUQ))
			{
				volumeConversionDictionary.ForEach(conversion => list.AddIfNotExists(conversion.Key, ConvertQuantity(currentQuantity, currentQuantityUQ, conversion.Key, volumeConversionDictionary)));
			}
			else if (alcoholConversionDictionary.ContainsKey(currentQuantityUQ))
			{
				alcoholConversionDictionary.ForEach(conversion => list.AddIfNotExists(conversion.Key, ConvertQuantity(currentQuantity, currentQuantityUQ, conversion.Key, alcoholConversionDictionary)));
			}
			else if (itemsConversionDictionary.ContainsKey(currentQuantityUQ))
			{
				itemsConversionDictionary.ForEach(conversion => list.AddIfNotExists(conversion.Key, ConvertQuantity(currentQuantity, currentQuantityUQ, conversion.Key, itemsConversionDictionary)));
			}
			else
			{
				list.AddIfNotExists(currentQuantityUQ, currentQuantity);
			}
		}

		decimal ConvertQuantity(decimal currentQuantity, string currentQuantityUQ, string requestedUq, ImmutableDictionary<string, decimal> conversionDictionary) => currentQuantity * (conversionDictionary[currentQuantityUQ] / conversionDictionary[requestedUq]);

		#region Immutable Collections

		readonly ImmutableDictionary<string, decimal> weightConversionDictionary = ConvertibleUnitsOfMeasure.WeightConversionDictionary;

		readonly ImmutableDictionary<string, decimal> volumeConversionDictionary = ConvertibleUnitsOfMeasure.VolumeConversionDictionary;

		readonly ImmutableDictionary<string, decimal> alcoholConversionDictionary = ConvertibleUnitsOfMeasure.AlcoholConversionDictionary;

		readonly ImmutableDictionary<string, decimal> itemsConversionDictionary = new Dictionary<string, decimal>()
			{
				{ CustomsUq.Number.NumberOfItems, 0.001m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems, 1m },
			}.ToImmutableDictionary();

		#endregion
	}
}
