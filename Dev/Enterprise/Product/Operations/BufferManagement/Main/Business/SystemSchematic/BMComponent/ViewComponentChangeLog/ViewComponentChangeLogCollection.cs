using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ViewComponentChangeLogCollection : ActiveBusinessObjectCollection<ViewComponentChangeLog>
	{
		public ViewComponentChangeLogCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNew => false;
	}
}
