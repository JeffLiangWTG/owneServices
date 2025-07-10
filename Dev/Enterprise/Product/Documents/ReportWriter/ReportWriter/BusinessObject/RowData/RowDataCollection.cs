using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class RowDataCollection : NonPersistentBusinessObjectCollection<RowData>
	{
		public RowDataCollection(IReportBizObjProvider parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly IReportBizObjProvider parent;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RowData(parent.GetReportBizObj());
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((RowData)child).Order = Count + 1;
		}

		#endregion
	}
}
