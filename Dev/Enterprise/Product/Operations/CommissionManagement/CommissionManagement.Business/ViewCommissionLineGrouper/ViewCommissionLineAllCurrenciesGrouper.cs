using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineAllCurrenciesGrouper<TLine> : ViewCommissionLineGrouper<TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		public override IEnumerable<IEnumerable<TLine>> GetGroupings(IEnumerable<TLine> lineProviders)
		{
			return lineProviders.GroupBy(x =>
				new
				{
					x.ViewCommissionLine.VCL_RX_NKTransactionCurrency,
					x.ViewCommissionLine.VCL_RX_NKCommissionCurrency,
					x.ViewCommissionLine.VCL_RX_NKPreferredPaymentCurrency,
					x.ViewCommissionLine.VCL_LocalToPreferredExchangeRate
				});
		}
	}
}
