using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceGroupHeader : BaseJobComInvoiceGroupHeader, Integration.Customs.KR.IJobComInvoiceGroupHeader, ICurrencyConverterDataProvider
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvChargeCollection<GroupInvoiceCharge> Charges => (JobComInvChargeCollection<GroupInvoiceCharge>)base.Charges;

		int ICurrencyConverterDataProvider.MaximumDaysToFallback => ((ICurrencyConverterDataProvider)JobDeclaration)?.MaximumDaysToFallback ?? 0;
		ExchangeRateType ICurrencyConverterDataProvider.RateType => ((ICurrencyConverterDataProvider)JobDeclaration)?.RateType ?? ExchangeRateType.Customs;

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new JobComInvChargeCollection<GroupInvoiceCharge>(this);
	}
}
