namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationDutyFeeProvider : DeclarationAdditionTaxProvider
	{
		DeclarationDutyFeeProvider(CusEntryLineFee entryLineFee) : base(entryLineFee)
		{
		}

		public static DeclarationDutyFeeProvider New(CusEntryLineFee entryLineFee) => entryLineFee == null ? null : new DeclarationDutyFeeProvider(entryLineFee);

		public override string TaxType => "0001";

		protected override string TaxRegime => randomInvoiceLine.DutyTaxRegime;

		public override decimal? IPTAmount
		{
			get
			{
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.FullCollection:
					case TaxRegimeList.Codes.Reduction:
						return cusEntryLineFee.CF_ChargeAmount;
					default:
						return null;
				}
			}
		}

		public override decimal? AgreementPercentual
		{
			get
			{
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.FullCollection:
					case TaxRegimeList.Codes.Reduction:
					case TaxRegimeList.Codes.Suspension:
						return IsPrimaryPreferenceFTA ? cusEntryLineFee.CF_Rate : 0;
					default:
						return 0;
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
					case TaxRegimeList.Codes.Reduction:
					case TaxRegimeList.Codes.Suspension:
					case TaxRegimeList.Codes.Exemption:
						return IsPrimaryPreferenceFTA || IsPrimaryPreferenceReducedRate || IsPrimaryPreferenceReductionMargin ? randomInvoiceLine.DutyVigentRateValue : cusEntryLineFee.CF_Rate;
					default:
						return 0;
				}
			}
		}

		public override decimal? TaxPayable
		{
			get
			{
				var rate = 0m;
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.FullCollection:
						rate = IsPrimaryPreferenceNormal || IsPrimaryPreferenceExTariff || IsPrimaryPreferenceFTA ? cusEntryLineFee.CF_Rate : 0;
						break;
					case TaxRegimeList.Codes.Reduction:
						rate = IsPrimaryPreferenceReducedRate ? cusEntryLineFee.CF_Rate : IsPrimaryPreferenceReductionMargin ? randomInvoiceLine.DutyVigentRateValue : 0;
						break;
					case TaxRegimeList.Codes.Suspension:
						rate = IsPrimaryPreferenceNormal || IsPrimaryPreferenceExTariff || IsPrimaryPreferenceFTA || IsPrimaryPreferenceReducedRate ? cusEntryLineFee.CF_Rate : IsPrimaryPreferenceReductionMargin ? randomInvoiceLine.DutyVigentRateValue : 0;
						break;
					case TaxRegimeList.Codes.Exemption:
						rate = IsPrimaryPreferenceNormal || IsPrimaryPreferenceExTariff || IsPrimaryPreferenceFTA || IsPrimaryPreferenceReducedRate ? cusEntryLineFee.CF_Rate : 0;
						break;
				}
				return cusEntryLineFee.EntryLine.CL_CustomsValue * rate / 100m;
			}
		}

		public override decimal? ReducedRatePercentage
		{
			get
			{
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.Reduction:
					case TaxRegimeList.Codes.Suspension:
						return IsPrimaryPreferenceReducedRate ? cusEntryLineFee.CF_Rate : 0;
					default:
						return 0;
				}
			}
		}

		public override decimal? IPTReductionPercentage
		{
			get
			{
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.Reduction:
					case TaxRegimeList.Codes.Suspension:
						return IsPrimaryPreferenceReductionMargin ? randomInvoiceLine.ReductionMarginRateValue : 0;
					default:
						return 0;
				}
			}
		}

		public override decimal? TariffACCalculatedValue
		{
			get
			{
				switch (TaxRegime)
				{
					case TaxRegimeList.Codes.Reduction:
					case TaxRegimeList.Codes.Suspension:
						return IsPrimaryPreferenceReducedRate || IsPrimaryPreferenceFTA ? cusEntryLineFee.EntryLine.CL_CustomsValue * cusEntryLineFee.CF_Rate / 100m : 0m;
					case TaxRegimeList.Codes.FullCollection:
						return IsPrimaryPreferenceFTA ? cusEntryLineFee.EntryLine.CL_CustomsValue * cusEntryLineFee.CF_Rate / 100m : 0m;
					default:
						return 0m;
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
					case TaxRegimeList.Codes.Reduction:
					case TaxRegimeList.Codes.Suspension:
					case TaxRegimeList.Codes.Exemption:
						return cusEntryLineFee.EntryLine.CL_CustomsValue * randomInvoiceLine.DutyVigentRateValue / 100m;
					default:
						return 0m;
				}
			}
		}

		bool IsPrimaryPreferenceFTA => randomInvoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.FreeTradeAgreement;

		bool IsPrimaryPreferenceReducedRate => randomInvoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.ReducedRate;

		bool IsPrimaryPreferenceReductionMargin => randomInvoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.ReductionMargin;

		bool IsPrimaryPreferenceNormal => randomInvoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.Normal;

		bool IsPrimaryPreferenceExTariff => randomInvoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.ExTariff;
	}
}

