using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ExchangeRateType RateTypeCore => JobDeclaration != null && JobDeclaration.IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;
	}
}
