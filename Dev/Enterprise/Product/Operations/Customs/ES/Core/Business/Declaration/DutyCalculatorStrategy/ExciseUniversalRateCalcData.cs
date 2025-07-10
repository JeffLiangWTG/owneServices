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
	public class ExciseUniversalRateCalcData : IUniversalRateCalcData
	{
		public ExciseUniversalRateCalcData(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			invoiceLine = Argument.NotNull(entryLine.RandomLine, nameof(entryLine.RandomLine));
		}
		readonly CusEntryLine entryLine;
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

		public decimal CustomsValue => entryLine.CL_CustomsValue;

		public IDictionary<string, decimal> UnitOfMeasureValueList => fUnitOfMeasureValueList ?? (fUnitOfMeasureValueList = GetNewUnitOfMeasureValueList());
		IDictionary<string, decimal> fUnitOfMeasureValueList;

		public IDictionary<string, decimal> CountrySpecificValueList => new Dictionary<string, decimal>() { { UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode, invoiceLine.ZG_TotalRetailPrice } };

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
				list.AddIfNotExists(CustomsUq.Weight.Kilogram, ConvertQuantity(currentQuantity, currentQuantityUQ, CustomsUq.Weight.Kilogram, weightConversionDictionary));
				list.AddIfNotExists(CustomsUq.Weight.Tonne, ConvertQuantity(currentQuantity, currentQuantityUQ, CustomsUq.Weight.Tonne, weightConversionDictionary));
			}
			else if (volumeConversionDictionary.ContainsKey(currentQuantityUQ))
			{
				list.AddIfNotExists(CustomsUq.Volume.Hectolitre, ConvertQuantity(currentQuantity, currentQuantityUQ, CustomsUq.Volume.Hectolitre, volumeConversionDictionary));
				list.AddIfNotExists(CustomsUq.Volume.Kilolitre, ConvertQuantity(currentQuantity, currentQuantityUQ, CustomsUq.Volume.Kilolitre, volumeConversionDictionary));
			}
			else if (itemsConversionDictionary.ContainsKey(currentQuantityUQ))
			{
				list.AddIfNotExists(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems, ConvertQuantity(currentQuantity, currentQuantityUQ, UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems, itemsConversionDictionary));
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

		readonly ImmutableDictionary<string, decimal> itemsConversionDictionary = new Dictionary<string, decimal>()
			{
				{ CustomsUq.Number.NumberOfItems, 0.001m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems, 1m },
			}.ToImmutableDictionary();

		#endregion
	}
}
