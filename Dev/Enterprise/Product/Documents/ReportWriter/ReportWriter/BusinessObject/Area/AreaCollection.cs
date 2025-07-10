using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class AreaCollection : NonPersistentBusinessObjectCollection<Area>
	{
		public AreaCollection(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly ReportBizObj parent;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Area(parent);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((Area)child).Order = Count + 1;
		}

		#endregion
	}
}
