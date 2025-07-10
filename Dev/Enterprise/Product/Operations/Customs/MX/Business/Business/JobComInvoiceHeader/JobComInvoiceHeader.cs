using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public partial class JobComInvoiceHeader : Customs.Business.BaseJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.BaseJobComInvoiceHeader.Schema
		{
			public const string ExchangeRateDate = nameof(JobComInvoiceHeader.ExchangeRateDate);
		}

		protected override ExchangeRateType RateTypeCore => IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Mexico;

		[ResourceStringData("FDF529CA-D573-41EB-9D8A-6D02EC5075B6", Caption = "[24] Tran. Nature", FullDescription = "The nature of the transaction.")]
		public override ZString JZ_ValuationCode
		{
			get => base.JZ_ValuationCode;
			set => base.JZ_ValuationCode = value;
		}

		[BusinessObjectTestExclude()]
		[ResourceStringData("0E4751F8-586F-43C0-A47F-BE473B305D3B", Caption = "Exchange Rate Date")]
		public ZDateTime ExchangeRateDate
		{
			get => EffectiveValuationDateCore;
			set
			{
				JZ_ValuationDateOverride = value;
				ExchangeRateDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeRateDateInfo
		{
			get { return GetZPropertyInfo(Schema.ExchangeRateDate); }
		}
	}
}
