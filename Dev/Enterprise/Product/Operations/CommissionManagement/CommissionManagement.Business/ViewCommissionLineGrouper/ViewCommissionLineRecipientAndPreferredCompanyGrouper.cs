using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineRecipientAndPreferredCompanyGrouper<TLine> : ViewCommissionLineGrouper<TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		public override IEnumerable<IEnumerable<TLine>> GetGroupings(IEnumerable<TLine> lineProviders)
		{
			return lineProviders.GroupBy(x =>
				new
				{
					x.ViewCommissionLine.VCL_GS_NKStaff,
					x.ViewCommissionLine.VCL_OH_Party,
					x.ViewCommissionLine.VCL_GC_PreferredPaymentCompany
				});
		}
	}
}
