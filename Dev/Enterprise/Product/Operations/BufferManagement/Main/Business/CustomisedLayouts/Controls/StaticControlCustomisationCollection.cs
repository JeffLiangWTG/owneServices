using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class StaticControlCustomisationCollection : ControlCustomisationBaseCollection<StaticControlCustomisation>
	{
		public StaticControlCustomisationCollection(BMControlCustomisation parent)
			: base(parent)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StaticControlCustomisation(Parent);
		}
	}
}
