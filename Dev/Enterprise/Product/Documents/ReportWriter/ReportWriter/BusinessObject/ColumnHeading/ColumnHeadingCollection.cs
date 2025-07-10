using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class ColumnHeadingCollection : NonPersistentBusinessObjectCollection<ColumnHeading>
	{
		public ColumnHeadingCollection(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly ReportBizObj parent;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ColumnHeading(parent);
		}

		#endregion
	}
}
