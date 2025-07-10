using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationCurrencyConverterDataProvider : ICurrencyConverterDataProvider
	{
		public JobDeclarationCurrencyConverterDataProvider(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}
		readonly JobDeclaration jobDeclaration;

		public ZDateTime DateOfValuation => DateOfValuationCore;

		public ExchangeRateType RateType => ExchangeRateType.Customs;

		public int MaximumDaysToFallback => BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack;

		public GlbCompany Company => jobDeclaration?.Company ?? GlbCompany.CurrentCompany;

		public ZString LocalCurrencyCodeOverride => jobDeclaration?.LocalCurrencyCode ?? Core.Constants.CurrencyCodes.Italy;

		public ZBool? IsReciprocalOverride => jobDeclaration?.IsReciprocalRates ?? GlbCompany.CurrentCompany.GC_IsReciprocal;

		protected virtual ZDateTime DateOfValuationCore => jobDeclaration?.DateOfValuation ?? ZDateTime.Today;
	}
}
