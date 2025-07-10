using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSDutyCalculatorStrategy : EU.Business.Declaration.DutyCalculatorStrategy
	{
		public CDSDutyCalculatorStrategy(JobDeclaration declaration)
		: base(declaration)
		{
		}

		ZBool ShouldSuspendDutyFromCalculation(EU.Business.Declaration.CusEntryLine entryLine) => GBCustomsDataRegistry.Instance.ExcludeSuspendedAndWaivedFeesFromCalculations.Value && ((CusEntryLine)entryLine).IsCustomsProcedureDutySuspendedOrWaived;

		ZBool ShouldSuspendVatFromCalculation(EU.Business.Declaration.CusEntryLine entryLine) => GBCustomsDataRegistry.Instance.ExcludeSuspendedAndWaivedFeesFromCalculations.Value && ((CusEntryLine)entryLine).IsCustomsProcedureVatSuspendedOrWaived;

		protected override void CalculateEntryLineFees(EU.Business.Declaration.CusEntryLine entryLine)
		{
			if (!ShouldSuspendDutyFromCalculation(entryLine))
			{
				base.CalculateEntryLineFees(entryLine);
			}
		}

		protected override void CalculateEntryLineVatFee(EU.Business.Declaration.CusEntryLine entryLine)
		{
			if (entryLine is CusEntryLine gbEntryLine && gbEntryLine.IsEuTariffToBeUsedForNorthernIreland)
			{
				CalculateCDSGBNorthernIrelandAtRiskVatFees(gbEntryLine);
			}
			else
			{
				if (!ShouldSuspendVatFromCalculation(entryLine))
				{
					base.CalculateEntryLineVatFee(entryLine);
				}
			}
		}

		void CalculateCDSGBNorthernIrelandAtRiskVatFees(CusEntryLine entryLine)
		{
			if (!entryLine.Fees.HasOverrideFeeOfGivenCode(UniversalReferenceConstants.RefCusRateCodes.Vat))
			{
				var vatCalculator = new CDSGBNorthernIrelandAtRiskEntryLineVatCalculator(entryLine);
				var calculatedVat = vatCalculator.CalculateVatFees();
				foreach (var calculatedVatFee in calculatedVat)
				{
					AddNewEntryLineFee(entryLine, calculatedVatFee.Code, ZString.Empty, calculatedVatFee);
				}
			}
		}

		protected override void SetChargeAmount(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, Customs.Business.IDutyCalculationIntermediateResult intermediateResult)
		{
			base.SetChargeAmount(entryLine, entryLineFee, intermediateResult);

			var line = (CusEntryLine)entryLine;
			var lineFee = (CusEntryLineFee)entryLineFee;

			if (line != null && lineFee != null)
			{
				if (line.IsNorthernIrelandDomestic)
				{
					if (entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat
						|| entryLineFee.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland)
					{
						entryLineFee.CF_ChargeAmount = ZDecimal.Zero;
					}

					if (!line.IsAtRisk)
					{
						if (lineFee.IsInNorthernIrelandDuty)
						{
							entryLineFee.CF_ChargeAmount = ZDecimal.Zero;
						}
					}
				}
			}
		}
	}
}
