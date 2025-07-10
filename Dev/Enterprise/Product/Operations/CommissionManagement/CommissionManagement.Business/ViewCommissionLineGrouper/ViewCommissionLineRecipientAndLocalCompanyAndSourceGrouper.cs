using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineRecipientAndLocalCompanyAndSourceGrouper<TLine> : ViewCommissionLineGrouper<TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		public override IEnumerable<IEnumerable<TLine>> GetGroupings(IEnumerable<TLine> lineProviders)
		{
			return lineProviders.GroupBy(x =>
				new
				{
					Entity = x.ViewCommissionLine.VCL_GS_NKStaff.IsEmpty ? (IZType)x.ViewCommissionLine.VCL_OH_Party : x.ViewCommissionLine.VCL_GS_NKStaff,
					x.ViewCommissionLine.VCL_GC_Company,
					x.ViewCommissionLine.VCL_GroupingSourceUniqueId
				});
		}
	}
}
