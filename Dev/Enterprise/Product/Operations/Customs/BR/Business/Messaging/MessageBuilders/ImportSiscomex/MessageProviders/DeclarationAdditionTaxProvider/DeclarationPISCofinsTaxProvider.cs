using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationPISCofinsTaxProvider : DeclarationAdditionTaxProvider
	{
		public DeclarationPISCofinsTaxProvider(CusEntryLineFee entryLineFee) : base(entryLineFee)
		{
		}

		public static DeclarationPISCofinsTaxProvider New(CusEntryLineFee entryLineFee) => entryLineFee == null ? null : new DeclarationPISCofinsTaxProvider(entryLineFee);

		public override string TaxType
		{
			get
			{
				switch (cusEntryLineFee.CF_ChargeType)
				{
					case Constants.RateTypes.PIS:
						return "0005";
					case Constants.RateTypes.Cofins:
						return "0006";
					default:
						return string.Empty;
				}
			}
		}

		protected override string TaxRegime => randomInvoiceLine.PisCofinsTaxRegime;

		public override decimal? IPTAmount
		{
			get
			{
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.FullCollection:
					case TaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_ChargeAmount;
					case TaxRegimeList.Codes.Suspension:
					case TaxRegimeList.Codes.Exemption:
					case TaxRegimeList.Codes.PaymentMade:
					case TaxRegimeList.Codes.NoIncident:
					case TaxRegimeList.Codes.Immunity:
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
					case TaxRegimeList.Codes.FullCollection:
					case TaxRegimeList.Codes.Suspension:
					case TaxRegimeList.Codes.Exemption:
						return cusEntryLineFee.CF_Rate;
					case TaxRegimeList.Codes.Reduction:
						return PisCofinsVigentRateValue;
					case TaxRegimeList.Codes.PaymentMade:
					case TaxRegimeList.Codes.NoIncident:
					case TaxRegimeList.Codes.Immunity:
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
					case TaxRegimeList.Codes.FullCollection:
					case TaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_ChargeAmount;
					case TaxRegimeList.Codes.Suspension:
					case TaxRegimeList.Codes.Exemption:
						return cusEntryLineFee.CF_BaseValue * (cusEntryLineFee.CF_Rate / 100m);
					case TaxRegimeList.Codes.PaymentMade:
					case TaxRegimeList.Codes.NoIncident:
					case TaxRegimeList.Codes.Immunity:
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
					case TaxRegimeList.Codes.FullCollection:
						return cusEntryLineFee.CF_ChargeAmount;
					case TaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_BaseValue * PisCofinsVigentRateValue;
					case TaxRegimeList.Codes.Suspension:
					case TaxRegimeList.Codes.Exemption:
					case TaxRegimeList.Codes.PaymentMade:
					case TaxRegimeList.Codes.NoIncident:
					case TaxRegimeList.Codes.Immunity:
						return decimal.Zero;
					default:
						return null;
				}
			}
		}

		protected override string SpecialRateCode => cusEntryLineFee.CF_ChargeType == Constants.RateTypes.PIS ? Constants.RateCodes.PIS : Constants.RateCodes.Cofins;

		ZDecimal PisCofinsVigentRateValue => cusEntryLineFee.CF_ChargeType == Constants.RateTypes.PIS ? randomInvoiceLine.PisVigentRateValue :
								cusEntryLineFee.CF_ChargeType == Constants.RateTypes.Cofins ? randomInvoiceLine.CofinsVigentRateValue : 0;

		public override decimal? ReducedRatePercentage
		{
			get
			{
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_Rate;
					default:
						return decimal.Zero;
				}
			}
		}

		public override decimal? SpecificReducedIPTRateValue => 0m;

		public override decimal? SpecificIPTCalculatedValue => 0m;
	}
}

