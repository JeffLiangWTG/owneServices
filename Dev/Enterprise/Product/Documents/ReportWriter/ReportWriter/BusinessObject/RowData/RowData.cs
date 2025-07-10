namespace Enterprise.ReportWriter
{
	public class RowData : AutoRowData, IReportBizObjProvider
	{
		public RowData(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public ColumnDataCollection Columns
		{
			get
			{
				if (columns == null)
				{
					columns = GetNewColumnDataCollection();
					RegisterEditableChildObject(columns);
				}
				return columns;
			}
		}
		ColumnDataCollection columns;

		protected virtual ColumnDataCollection GetNewColumnDataCollection()
		{
			return new ColumnDataCollection(this);
		}

		ReportBizObj IReportBizObjProvider.GetReportBizObj()
		{
			return parent;
		}
		public readonly ReportBizObj parent;
	}
}
