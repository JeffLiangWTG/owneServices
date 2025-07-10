using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class HarbourFeeUniversalRateData : IUniversalRateCalcData
	{
		readonly HarbourFeeWrapper wrapper;

		public HarbourFeeUniversalRateData(HarbourFeeWrapper wrapper, ZString formula)
		{
			this.wrapper = Argument.NotNull(wrapper, "wrapper");
			CustomsValueFormula = Argument.NotNull(formula, "formula");
		}

		public DateTime DateOfValuation => wrapper.DateOfValuation;

		public decimal CustomsValue => wrapper.CustomsValue;

		public decimal ValueForDuty
		{
			get
			{
				var result = CustomsValue;
				if (!CustomsValueFormula.IsEmpty)
				{
					if (!cachedValueForDuty.HasValue)
					{
						var calcResult = new UniversalRateCalculator(CustomsValueFormula, this).Calculate();
						cachedValueForDuty = Utilities.Round(calcResult, 3);
					}
					result = cachedValueForDuty.Value;
				}
				return result;
			}
		}
		ZDecimal? cachedValueForDuty;

		internal ZString CustomsValueFormula
		{
			get { return customsValueFormula; }
			set
			{
				customsValueFormula = value;
				cachedValueForDuty = null;
			}
		}
		ZString customsValueFormula;

		public IDictionary<string, decimal> UnitOfMeasureValueList
		{
			get
			{
				if (fUnitOfMeasureValueList == null)
				{
					fUnitOfMeasureValueList = new Dictionary<string, decimal>();
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.TwentyFootLCL, wrapper.NumberOfTwentyFootLCLContainers);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.FortyFootLCL, wrapper.NumberOfFortyFootLCLContainers);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.FortyFiveFootLCL, wrapper.NumberOfFortyFiveFootLCLContainers);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.TonnesLCL, wrapper.TotalLCLContainersMassInTonnes);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.TwentyFootFCL, wrapper.NumberOfTwentyFootFCLContainers);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.FortyFootFCL, wrapper.NumberOfFortyFootFCLContainers);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.FortyFiveFootFCL, wrapper.NumberOfFortyFiveFootFCLContainers);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.TonnesFCL, wrapper.TotalFCLContainersMassInTonnes);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.DangerousGoods, wrapper.IsDangerousGoods ? 1 : 0);
					fUnitOfMeasureValueList.Add(FRConstants.HarbourRateFormulaCodes.Container, wrapper.ContainerCount);
				}
				return fUnitOfMeasureValueList;
			}
		}

		IDictionary<string, decimal> fUnitOfMeasureValueList;

		public IDictionary<string, decimal> CountrySpecificValueList { get; } = new Dictionary<string, decimal>();

		public IList<Tuple<string, string>> AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();
	}
}
