using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public abstract class ViewCommissionLineGrouper<TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public abstract IEnumerable<IEnumerable<TLine>> GetGroupings(IEnumerable<TLine> lineProviders);
	}
}
