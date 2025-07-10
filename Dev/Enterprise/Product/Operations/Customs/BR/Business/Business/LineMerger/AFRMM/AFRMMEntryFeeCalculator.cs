using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class AFRMMEntryFeeCalculator : BaseEntryFeeCalculator
	{
		public AFRMMEntryFeeCalculator(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override string FeeType => Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax;

		protected override IFeeApportionManager ApportionManager => new AFRMMFeeApportionManager();

		protected internal override ZDecimal CalculateTotalFee()
		{
			var totalChargeAmount = 0m;
			var feeRate = EntryHeader.EntryInstruction?.CEI_AFRMMRateOverride ?? ZDecimal.Zero;
			var fixedValue = EntryHeader.EntryInstruction?.CEI_UtilizationFeeOverride ?? ZDecimal.Zero;

			foreach (var entryLine in EntryHeader.MergedLines)
			{
				var totalAmountOnEntryLine = 0m;
				var invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>();

				foreach (var chargeType in ChargeCodesForAFRMMCalculation)
				{
					totalAmountOnEntryLine += invoiceLines.GetTotalChargesAmountOnInvoiceLines(c => c.J7_ChargeType == chargeType, currency: EntryHeader.LocalCurrency);	
				}
				totalChargeAmount += totalAmountOnEntryLine;

				entryLine.Fees.GetOrAddFeeByFeeType(FeeType);
			}

			EntryHeader.MergedLines.Select(x => x.Fees.GetOrAddFeeByFeeType(FeeType)).ForEach(x => { x.CF_BaseValue = totalChargeAmount + fixedValue; x.CF_Rate = feeRate; x.CF_MethodOfCalculation = Constants.MethodOfCalculation.Percentage; });

			return (totalChargeAmount * (feeRate / 100)) + fixedValue;
		}

		static IEnumerable<ZString> ChargeCodesForAFRMMCalculation
		{
			get
			{
				yield return ImportCustomsChargeTypeList.Codes.FreightComponents;
				yield return ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
				yield return ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			}
		}
	}
}
