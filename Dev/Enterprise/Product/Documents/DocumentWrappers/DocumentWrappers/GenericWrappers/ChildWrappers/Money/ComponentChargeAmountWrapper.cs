using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("WithTax")]
	[WrapperTypeName("Charge Breakdown")]
	public sealed class ComponentChargeAmountWrapper : GenericWrapper
	{
		public ComponentChargeAmountWrapper(ZDecimal net, ZDecimal tax, ICurrency currency, BusinessObjectFactory factory)
			: this(net, tax, net + tax, currency, factory) { }

		public ComponentChargeAmountWrapper(ZDecimal withoutTax, ZDecimal tax, ZDecimal withTax, ICurrency currency, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.withoutTax = withoutTax;
			this.tax = tax;
			this.withTax = withTax;
			this.currency = currency;
		}

		public MoneyWrapper WithoutTax
		{
			get { return new MoneyWrapper(new Money(withoutTax, currency), Factory); }
		}

		public MoneyWrapper Tax
		{
			get { return new MoneyWrapper(new Money(tax, currency), Factory); }
		}

		public MoneyWrapper WithTax
		{
			get { return new MoneyWrapper(new Money(withTax, currency), Factory); }
		}

		public CurrencyWrapper Currency
		{
			get { return new CurrencyWrapper(currency, Factory); }
		}

		readonly ZDecimal withoutTax;
		readonly ZDecimal tax;
		readonly ZDecimal withTax;
		readonly ICurrency currency;
	}
}
