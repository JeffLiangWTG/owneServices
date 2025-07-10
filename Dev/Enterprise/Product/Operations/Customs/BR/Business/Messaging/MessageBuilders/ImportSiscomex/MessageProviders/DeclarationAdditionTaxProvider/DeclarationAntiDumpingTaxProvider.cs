using System.Linq;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationAntiDumpingTaxProvider : DeclarationAdditionTaxProvider
	{
		DeclarationAntiDumpingTaxProvider(CusEntryLineFee entryLineFee) : base(entryLineFee)
		{
		}

		public static DeclarationAntiDumpingTaxProvider New(CusEntryLineFee entryLineFee)
		{
			return entryLineFee == null || entryLineFee.CF_Rate.IsEmpty ? null : new DeclarationAntiDumpingTaxProvider(entryLineFee);
		}

		public override string TaxType => "0003";

		public override decimal? AgreementPercentualNormal => cusEntryLineFee.IsQuantityPerUnit ? 0m : cusEntryLineFee?.CF_Rate;

		public override decimal? TaxPayable => cusEntryLineFee.CF_ChargeAmount;

		public override string DirectTypeCode => "1";

		protected override string SpecialRateCode => Constants.RateCodes.Antidumping;

		public override decimal? IPTCalculatedValue => null;

		public override int? SpecificRateUnitQuantity => cusEntryLineFee.IsQuantityPerUnit ? cusEntryLineFee?.CF_BaseValue.ToZInt() : 0;

		public override decimal? SpecificIPTRateValue => cusEntryLineFee.IsQuantityPerUnit ? cusEntryLineFee?.CF_Rate : 0;

		public override string SpecificUQ => cusEntryLineFee.IsQuantityPerUnit ? randomInvoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().FirstOrDefault(x => x.TaxGroup == SpecialRateCode)?.UnitOfMeasureDescInPortugueseBrazil.ToUpper() : null;
	}
}
