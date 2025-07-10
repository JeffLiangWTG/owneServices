using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CalculateLiabilityBizObj : AutoCalculateLiabilityNonPersistentBizObj
	{
		public CalculateLiabilityBizObj(BusinessObjectFactory factory, NctsGuarantee guarantee) : base(factory)
		{
			Guarantee = Argument.NotNull(guarantee, nameof(guarantee));
			DefaultValues();
		}

		public NctsGuarantee Guarantee { get; }

		public override ZDecimal TotalValue
		{
			get => base.TotalValue;
			set
			{
				var oldValue = TotalValue;
				base.TotalValue = value;
				if (!IsCopying && oldValue != value)
				{
					UpdateLiabilityAmount();
				}
			}
		}

		public override ZInt LiabilityPercentage
		{
			get => base.LiabilityPercentage;
			set
			{
				var oldValue = LiabilityPercentage;
				base.LiabilityPercentage = value;
				if (!IsCopying && oldValue != value)
				{
					UpdateLiabilityAmount();
				}
			}
		}

		[ReadOnly(true)]
		public override ZDecimal LiabilityAmount { get => base.LiabilityAmount; set => base.LiabilityAmount = value; }

		public override ZBool UseDutiesAndTaxes
		{
			get => base.UseDutiesAndTaxes;
			set
			{
				var oldValue = UseDutiesAndTaxes;
				base.UseDutiesAndTaxes = value;
				if (!IsCopying && oldValue != value)
				{
					UseMonetaryValue = !value;
					UpdateTotalValueViaCalculationMethod();
				}
			}
		}

		public override ZBool UseMonetaryValue
		{
			get => base.UseMonetaryValue;
			set
			{
				var oldValue = UseMonetaryValue;
				base.UseMonetaryValue = value;
				if (!IsCopying && oldValue != value)
				{
					UseDutiesAndTaxes = !value;
					UpdateTotalValueViaCalculationMethod();
				}
			}
		}

		void UpdateLiabilityAmount()
		{
			LiabilityAmount = Utilities.Round(TotalValue * LiabilityPercentage / 100m, LiabilityAmount_Scale);
		}

		public void UpdateLiabilityAmountOnGuarantee() => Guarantee.PW_BondAmount = LiabilityAmount;

		public void SetGuaranteeOverrideToFalse() => Guarantee.PW_Override = false;

		[List(nameof(Guarantee) + "." + nameof(NctsGuarantee.Lookups) + "." + nameof(NctsGuaranteeLookups.Currencies))]
		public override ZString Currency => base.Currency;

		protected override ZString GetCurrency() => GuaranteeCurrency?.Code;

		RefCurrency GuaranteeCurrency => Guarantee.Currency;

		NctsHeader NctsHeader => nctsHeader ??= Guarantee.NctsHeader;
		NctsHeader nctsHeader;

		GuaranteeConfiguration GuaranteeConfiguration => guaranteeConfiguration ??= NctsHeader.Configuration.GuaranteeConfiguration;
		GuaranteeConfiguration guaranteeConfiguration;

		void DefaultValues()
		{
			using (SuspendSettingHasChanges())
			{
				LiabilityPercentage = GuaranteeConfiguration.DefaultPercentageForLiabilityAmountCalculation;
				if (GuaranteeConfiguration.UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods)
				{
					UseDutiesAndTaxes = true;
				}
				else
				{
					TotalValue = Guarantee.PW_BondAmount;
				}
			}
		}

		void UpdateTotalValueViaCalculationMethod()
		{
			var converter = CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, roundToTargetCurrencyDecimals: false);
			var result = UseDutiesAndTaxes
				? converter.ConvertExact(new Money(NctsHeader.ApportionedAmount, GlbCompany.CurrentCompany.LocalCurrency), GuaranteeCurrency)
				: CalculateFromMonetaryValue();
			TotalValue = Utilities.Round(result.Amount, TotalValue_Scale);

			Money CalculateFromMonetaryValue()
			{
				var result = Money.Empty;
				var departureGoodsItems = NctsHeader.DepartureGoodsItems;
				if (departureGoodsItems.Count > 0 && GuaranteeCurrency is RefCurrency guaranteeCurrency)
				{
					var totalMonetaryValueOfItems = departureGoodsItems.Select(d => new Money(d.BY_MonetaryValue, d.Currency)).Aggregate(Money.Empty, converter.Add);
					result = converter.ConvertExact(totalMonetaryValueOfItems, guaranteeCurrency);
				}
				return result;
			}
		}
	}
}
