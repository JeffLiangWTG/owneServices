using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocJobExchangeRate : DocumentWrapper
	{
		DocJobExchangeRate(ExchangeRate exchangeRate, BusinessObjectFactory factoryToWrap)
			: base(exchangeRate, factoryToWrap)
		{
		}

		public static DocJobExchangeRate New(ExchangeRate exchangeRate, BusinessObjectFactory factoryToWrap)
		{
			if (exchangeRate == null)
			{
				return null;
			}
			else
			{
				return new DocJobExchangeRate(exchangeRate, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZDecimal BuyRate
		{
			get { return ExchangeRate.JF_BaseRate; }
		}

		public DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(ExchangeRate.Job, Factory); }
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(ExchangeRate.RateCurrency, Factory); }
		}

		public ZString CurrencyCode
		{
			get { return (Currency != null) ? Currency.Code : ZString.Empty; }
		}

		public ZString Organization => ExchangeRate.Org?.OH_Code ?? ZString.Empty;

		public ZString OrgType => ExchangeRate.JF_OrgType.IsEmpty ? new ZString("ALL") : ExchangeRate.JF_OrgType;

		public ZString CfxPercent => ExchangeRate.JF_CFXPercent.ToString() + "%";

		public ZString CfxMinimum => ExchangeRate.JF_CFXMinimum.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals);

		public ZBool IsCfxApplied => !ExchangeRate.IsCFXNotApplied;

		public ZDecimal SellRate
		{
			get { return ExchangeRate.JF_SellRate; }
		}

		public ZDecimal SellRateAgent
		{
			get { return ExchangeRate.JF_SellRate; }
		}

		#region Implementation

		ExchangeRate ExchangeRate
		{
			get { return (ExchangeRate)WrappedObject; }
		}

		#endregion
	}
}
