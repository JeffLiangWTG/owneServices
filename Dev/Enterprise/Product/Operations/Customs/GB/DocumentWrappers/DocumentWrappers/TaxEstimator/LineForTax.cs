using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.GB.Business.GBUniversalReferenceConstants;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class LineForTax : DocSADHLine
	{
		public LineForTax(CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
			PrepareDuties();
			InitialiseVat();
		}

		protected new Business.Declaration.JobDeclaration Declaration => (Business.Declaration.JobDeclaration)base.Declaration;

		public ZString ExchangeRateDetails
		{
			get
			{
				return EntryLine.InvoiceCurrency == null || EntryLine.InvoiceCurrency.RX_Code == Core.Constants.CurrencyCodes.UnitedKingdom
				  ? ZString.Empty
				  : ZString.Format("@{0}/£", EntryLine.RandomLine.InvoiceHeader.JZ_InvoiceCurrExRate.ToStringTrimZeros());
			}
		}

		public ZString Box33CommodityCodeIncludingTaric
		{
			get
			{
				return string.Concat(Box33CommodityCode,
									 Box33ECSupplement.IsEmpty ? string.Empty : ("/" + Box33ECSupplement),
									 Box33ECSupplement2.IsEmpty ? string.Empty : ("/" + Box33ECSupplement2));
			}
		}

		/// <summary>
		/// Sum of all duties.  e.g. A00=£5, A30=£15, TotalDutyDue=£20.
		/// </summary>
		public ZDecimal TotalDutyDue
		{
			get;
			protected set;
		}

		/// <summary>
		/// The base amount on which the line's VATR is caclulated.
		/// </summary>
		public ZDecimal VatableAmount
		{
			get { return new ZDecimal(TotalDutyDue + SummedValueForVat); }
		}

		/// <summary>
		/// English explanation of the rate of VAT.  Incorporates the name of the AccTaxId code.  e.g. A=LOWVAT
		/// </summary>
		public ZString VATRateSummary
		{
			get;
			private set;
		}

		/// <summary>
		/// Percent, e.g. 20, for the rate of VAT for this line.  Found from B00 tax line's rate, looked up against a AccTaxId.  Z-->0%, A-->5%, S-->20%
		/// </summary>
		public ZDecimal VatPercent
		{
			get;
			private set;
		}

		/// <summary>
		/// Amount of VAT payable. e.g. £100 uplifted VATable amount, 20% rate, gives £20.
		/// </summary>
		public ZDecimal VatDue
		{
			get { return (VatPercent / 100) * VatableAmount; }
		}

		/// <summary>
		/// A box47-like worksheet showing duties due.
		/// String showing duty type, rate, rate percent and amount due.  One row per non-VAT box 47.
		/// e.g. A00 F 5% 500.00
		/// </summary>
		public ZString Duties
		{
			get;
			protected set;
		}

		public ZDecimal CustomsValue
		{
			get { return EntryLine.CustomsValue.Amount; }
		}

		public ZDecimal SummedValueForVat
		{
			get { return summedValueForVat ?? (summedValueForVat = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.JI_Calc_ValueForVat)).Value; }
		}
		ZDecimal? summedValueForVat;

		protected void PrepareDuties()
		{
			var workSheet = new ZStringBuilder();
			var dutyCalculatorStrategySimulator = new DutyCalculatorStrategySimulator(EntryLine.Header.Declaration, EntryLine);
			var results = dutyCalculatorStrategySimulator.CalculateEntryLineFeesForTaxEstimator();
			TotalDutyDue = ZDecimal.Zero;
			foreach (var fee in results)
			{
				var result = fee.result;
				workSheet.AppendLine(string.Format("{0}												{1}												{2}												{3}",
						fee.code, result.MethodOfCalculation, result.AdjustedRate.ToStringTrimZeros(2), result.Amount.ToString(2)));

				TotalDutyDue += result.Amount;
			}
			Duties = workSheet.ToString();
		}

		void InitialiseVat()
		{
			var vatTaxLine = Box47Taxes.Cast<DocSADHLineTax>().FirstOrDefault(t => t.G4_Type == Chief.ChiefConstants.VatCode);
			if (vatTaxLine != null)
			{
				string enterpriseVatCode = null;
				switch (vatTaxLine.G4_RateDuty)
				{
					// TODO - get these from database // one day
					case TaxRateVATDutyListImport.Codes.VATTheGoodsAreZeroRated:
						enterpriseVatCode = TaxOrFeeTypeCode.ZeroRated;
						break;
					case TaxRateVATDutyListImport.Codes.VAT5PercentLowerRateForCertainGoods:
						enterpriseVatCode = TaxOrFeeTypeCode.ReducedRate;
						break;
					case TaxRateVATDutyListImport.Codes.VATTheGoodsAreLiableToVATAtTheStandardRate:
						enterpriseVatCode = TaxOrFeeTypeCode.StandardRate;
						break;
					case TaxRateVATDutyListImport.Codes.VATTheGoodsAreExemptFromVAT:
						enterpriseVatCode = TaxOrFeeTypeCode.VATExempt;
						break;
				}

				if (enterpriseVatCode != null)
				{
					var query = new ZQuery(RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, Declaration.CountryCode);
					query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_Code, enterpriseVatCode);
					query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, Declaration.DateOfValuation);
					query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, Declaration.DateOfValuation);
					var refCusTaxOrFee = Factory.Load<Universal.RefCusTaxOrFee>(query).FirstOrDefault();
					if (refCusTaxOrFee != null)
					{
						VatPercent = refCusTaxOrFee.ZZF_Value * 100;
						VATRateSummary = string.Concat(vatTaxLine.G4_RateDuty, " = ", refCusTaxOrFee.ZZF_Description);
					}
				}
			}
		}
	}
}
