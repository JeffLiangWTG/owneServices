using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class GroupByColumnCollection : NonPersistentBusinessObjectCollection<GroupByColumn>
	{
		public GroupByColumnCollection(Area parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly Area parent;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GroupByColumn(Factory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((GroupByColumn)child).Order = Count + 1;
		}

		#endregion
	}
}
