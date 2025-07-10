namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationIPITaxProvider : DeclarationAdditionTaxProvider
	{
		DeclarationIPITaxProvider(CusEntryLineFee entryLineFee) : base(entryLineFee)
		{
		}

		public static DeclarationIPITaxProvider New(CusEntryLineFee entryLineFee) => entryLineFee == null ? null : new DeclarationIPITaxProvider(entryLineFee);

		public override string TaxType => "0002";

		protected override string TaxRegime => randomInvoiceLine.IPITaxRegime;

		public override decimal? IPTAmount
		{
			get
			{
				switch (TaxRegime)
				{
					case IPITaxRegimeList.Codes.FullCollection:
					case IPITaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_ChargeAmount;
					case IPITaxRegimeList.Codes.Suspension:
					case IPITaxRegimeList.Codes.Exemption:
					case IPITaxRegimeList.Codes.NonTaxable:
						return decimal.Zero;
					default:
						return null;
				}
			}
		}

		public override decimal? TaxPayable
		{
			get
			{
				switch (TaxRegime)
				{
					case IPITaxRegimeList.Codes.FullCollection:
					case IPITaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_ChargeAmount;
					case IPITaxRegimeList.Codes.Suspension:
					case IPITaxRegimeList.Codes.Exemption:
						return cusEntryLineFee.CF_BaseValue * (cusEntryLineFee.CF_Rate / 100);
					case IPITaxRegimeList.Codes.NonTaxable:
						return decimal.Zero;
					default:
						return null;
				}
			}
		}

		public override decimal? AgreementPercentualNormal
		{
			get
			{
				switch (TaxRegime)
				{
					case IPITaxRegimeList.Codes.FullCollection:
					case IPITaxRegimeList.Codes.Suspension:
					case IPITaxRegimeList.Codes.Exemption:
						return cusEntryLineFee.CF_Rate;
					case IPITaxRegimeList.Codes.Reduction:
						return randomInvoiceLine.IPIVigentRateValue;
					case IPITaxRegimeList.Codes.NonTaxable:
						return decimal.Zero;
					default:
						return null;
				}
			}
		}

		public override decimal? BaseValue
		{
			get
			{
				switch (TaxRegime)
				{
					case IPITaxRegimeList.Codes.FullCollection:
					case IPITaxRegimeList.Codes.Reduction:
					case IPITaxRegimeList.Codes.Suspension:
					case IPITaxRegimeList.Codes.Exemption:
						return cusEntryLineFee.CF_BaseValue;
					case IPITaxRegimeList.Codes.NonTaxable:
						return decimal.Zero;
					default:
						return null;
				}
			}
		}

		public override decimal? IPTCalculatedValue
		{
			get
			{
				switch (TaxRegime)
				{
					case IPITaxRegimeList.Codes.FullCollection:
						return cusEntryLineFee.CF_ChargeAmount;
					case IPITaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_BaseValue * randomInvoiceLine.IPIVigentRateValue;
					case IPITaxRegimeList.Codes.Suspension:
					case IPITaxRegimeList.Codes.Exemption:
					case IPITaxRegimeList.Codes.NonTaxable:
						return decimal.Zero;
					default:
						return null;
				}
			}
		}

		protected override string SpecialRateCode => Constants.RateCodes.IPI;

		public override decimal? ReducedRatePercentage
		{
			get
			{
				switch (TaxRegime)
				{
					case IPITaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_Rate;
					default:
						return decimal.Zero;
				}
			}
		}

		public override decimal? SpecificReducedIPTRateValue => 0m;

		public override decimal? SpecificIPTCalculatedValue => 0m;

		public override int? QuantityMLContainer => 0;

		public override string IPIComplementaryNote => randomInvoiceLine.JI_ComplementaryNote;

		public override string IPITaxRegime => randomInvoiceLine.IPITaxRegime;

		public override string RecipientTypeCode => randomInvoiceLine.JI_CustomsSecondUnitQty;
	}
}
