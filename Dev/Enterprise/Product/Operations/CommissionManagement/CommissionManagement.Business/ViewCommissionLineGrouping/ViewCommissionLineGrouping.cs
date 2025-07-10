using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineGrouping : CommissionLineGrouping<ViewCommissionLineGrouping, ViewCommissionLine>
	{
		#region Constructors

		public ViewCommissionLineGrouping(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public ViewCommissionLineGrouping(BusinessObjectFactory factory, ViewCommissionLineGrouper<ViewCommissionLine>[] subGroupers)
			: base(factory, subGroupers)
		{
		}

		#endregion

		#region SubGroupings

		protected override CommissionLineGroupingCollection<ViewCommissionLineGrouping, ViewCommissionLine> GetNewSubGroupingCollection(BusinessObjectFactory factory)
		{
			return new ViewCommissionLineGroupingCollection(factory);
		}

		public new ViewCommissionLineGroupingCollection SubGroupingCollection
		{
			get { return (ViewCommissionLineGroupingCollection)base.SubGroupingCollection; }
		}

		public new IEnumerable<ViewCommissionLineGrouping> SubGroupings
		{
			get { return base.SubGroupings.Cast<ViewCommissionLineGrouping>(); }
		}

		#endregion
	}
}
