using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public abstract class DeclarationAdditionTaxProvider : IDeclarationAdditionTax
	{
		public DeclarationAdditionTaxProvider(CusEntryLineFee entryLineFee)
		{
			cusEntryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
			randomInvoiceLine = cusEntryLineFee.EntryLine.RandomLine as JobComInvoiceLine;
		}
		protected readonly CusEntryLineFee cusEntryLineFee;
		protected readonly JobComInvoiceLine randomInvoiceLine;

		public static IEnumerable<IDeclarationAdditionTax> New(CusEntryLine entryLine, params string[] chargeTypes)
		{
			foreach (var chargeType in chargeTypes)
			{
				IDeclarationAdditionTax provider = null;

				var entryLineFee = entryLine?.Fees.GetElementWithThisCode(chargeType);
				switch (chargeType)
				{
					case ChargeTypesList.Codes.DTY:
						provider = DeclarationDutyFeeProvider.New(entryLineFee);
						break;
					case Constants.RateTypes.Antidumping:
						provider = DeclarationAntiDumpingTaxProvider.New(entryLineFee);
						break;
					case Constants.RateTypes.IPI:
						provider = DeclarationIPITaxProvider.New(entryLineFee);
						break;
					case Constants.RateTypes.PIS:
					case Constants.RateTypes.Cofins:
						provider = DeclarationPISCofinsTaxProvider.New(entryLineFee);
						break;
				}

				if (provider != null)
				{
					yield return provider;
				}
			}
		}

		public abstract string TaxType { get; }

		public abstract decimal? TaxPayable { get; }

		public string RateType => cusEntryLineFee.IsQuantityPerUnit ? "2" : "1";

		public virtual decimal? BaseValue => cusEntryLineFee.CF_BaseValue;

		public virtual decimal? IPTAmount => cusEntryLineFee.CF_ChargeAmount;

		public virtual decimal? IPTCalculatedValue => TaxPayable;

		public virtual decimal? AgreementPercentual => null;

		public virtual decimal? AgreementPercentualNormal => null;

		public virtual decimal? ReducedRatePercentage => null;

		public virtual int? SpecificRateUnitQuantity => cusEntryLineFee.IsQuantityPerUnit ? cusEntryLineFee?.CF_BaseValue.ToZInt() : null;

		public virtual string SpecificUQ
		{
			get
			{
				if (cusEntryLineFee.IsQuantityPerUnit && SpecialRateCode != null)
				{
					return randomInvoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().FirstOrDefault(x => x.TaxGroup == SpecialRateCode)?.UnitOfMeasure ?? null;
				}
				return null;
			}
		}

		protected virtual string SpecialRateCode => null;

		public virtual decimal? SpecificIPTRateValue => cusEntryLineFee.IsQuantityPerUnit ? cusEntryLineFee?.CF_ChargeAmount : null;

		public virtual decimal? SpecificReducedIPTRateValue => null;

		public virtual decimal? TariffACCalculatedValue => 0m;

		public virtual decimal? SpecificIPTCalculatedValue => null;

		public virtual string IPITaxRegime => null;

		public virtual string IPIComplementaryNote => null;

		public virtual int? QuantityMLContainer => null;

		public virtual string DirectTypeCode => null;

		protected virtual string TaxRegime => null;

		public virtual decimal? IPTReductionPercentage => null;

		public virtual string RecipientTypeCode => null;
	}
}

