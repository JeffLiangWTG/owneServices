using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ExciseCalculator : IExtraFeeCalculator
	{
		public ExciseCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			invoiceLine = entryLine.RandomLine;
		}
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine invoiceLine;

		public ZString RateCode => invoiceLine.ZG_ExciseCode;

		public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
		{
			var fees = new List<IDutyCalculationIntermediateResult>();
			var exciseRate = invoiceLine.CurrentExciseRate;
			if (exciseRate != null)
			{
				var universalRateData = new ExciseUniversalRateCalcData(entryLine);
				var formattedFormula = exciseRate.ZZ2_RateFormula;
				if (exciseRate.RateCode == RefCusRateCode.FluorinatedGases &&
					exciseRate.ZZ2_RateFormula.Contains(ReservedRateFormulaValue.GlobalWarmingPotential))
				{
					const string defaultFluorinatedGasesFormula = "100*[GF]";
					var pcaValue = invoiceLine.ZG_GlobalWarmingPotential;
					formattedFormula = pcaValue.IsEmpty ?
										(ZString)defaultFluorinatedGasesFormula :
										exciseRate.ZZ2_RateFormula.Replace(ReservedRateFormulaValue.GlobalWarmingPotential, pcaValue.ToString());
				}
				var calculatedDuty = RateCalculationVisitor.CalculateParticipatingFees(universalRateData, formattedFormula);
				fees.AddRange(calculatedDuty.IntermediateResults);
			}

			return fees;
		}
	}
}
