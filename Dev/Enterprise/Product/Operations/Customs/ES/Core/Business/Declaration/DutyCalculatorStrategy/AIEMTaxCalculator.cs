using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AIEMTaxCalculator : IExtraFeeCalculator
	{
		public AIEMTaxCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		public ZString RateCode => UniversalReferenceConstants.RefCusRateCode.AIEM;

		public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
		{
			var aiemTax = new List<IDutyCalculationIntermediateResult>();
			var aiemTariffs = entryLine.RandomLine.UniversalTariff?.ChildTariffs.Where(x => x.RelatedTariffType.ZZI_TariffType == UniversalReferenceConstants.RefCusTariffType.AIEM).Select(x => x.RelatedTariffFrom) ?? Array.Empty<TariffView>();
			var aiemType = entryLine.RandomLine.ZG_AIEMType;
			if (aiemTariffs.Any() && !aiemType.IsEmpty)
			{
				var rate = aiemTariffs.FirstOrDefault(x => x.ZZ1_TariffCode.Contains(aiemType))?.Rates.FirstOrDefault();
				if (rate != null)
				{
					var calculatedDuties = RateCalculationVisitor.CalculateParticipatingFees(new AIEMRateCalcData(entryLine, rate), rate.ZZ2_RateFormula);
					aiemTax.AddRange(calculatedDuties.IntermediateResults);
				}
			}
			return aiemTax;
		}
	}
}
