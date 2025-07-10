using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class ColumnDataCollection : NonPersistentBusinessObjectCollection<ColumnData>
	{
		public ColumnDataCollection(IReportBizObjProvider parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly IReportBizObjProvider parent;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ColumnData(parent.GetReportBizObj());
		}

		#endregion
	}
}
