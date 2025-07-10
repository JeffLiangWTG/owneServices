using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class ViewCommissionLineGrouperForTest<TLine> : ViewCommissionLineGrouper<TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		public ViewCommissionLineGrouperForTest(Func<IEnumerable<TLine>, IEnumerable<IEnumerable<TLine>>> getGroupingsDelegate)
		{
			this.getGroupingsDelegate = getGroupingsDelegate;
		}

		readonly Func<IEnumerable<TLine>, IEnumerable<IEnumerable<TLine>>> getGroupingsDelegate;

		public override IEnumerable<IEnumerable<TLine>> GetGroupings(IEnumerable<TLine> lineProviders)
		{
			return getGroupingsDelegate(lineProviders);
		}
	}
}
