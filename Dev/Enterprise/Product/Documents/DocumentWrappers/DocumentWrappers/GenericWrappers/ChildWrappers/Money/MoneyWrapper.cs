using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("AmountAndCurrencyCode")]
	public class MoneyWrapper : GenericWrapper
	{
		public MoneyWrapper(Money amount, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fAmount = amount ?? Money.Empty;
		}

		public ZDecimal Amount
		{
			get { return fAmount.Amount; }
		}

		public ZString AmountAndCurrencyCode
		{
			get { return fAmount.IsEmpty ? "" : fAmount.ToString(); }
		}

		public CurrencyWrapper Currency
		{
			get
			{
				if (fCurrency == null)
				{
					fCurrency = new CurrencyWrapper((fAmount.IsEmpty ? null : fAmount.Currency), Factory);
				}
				return fCurrency;
			}
		}

		#region Implementation
		readonly Money fAmount;
		internal Money AmountAsMoney
		{
			get { return fAmount; }
		}
		CurrencyWrapper fCurrency;
		#endregion
	}
}
