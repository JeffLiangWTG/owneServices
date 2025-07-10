using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLineCollection : ControlCustomisationBaseCollection<BMControlCustomisationLine>
	{
		public BMControlCustomisationLineCollection(BMControlCustomisation parent)
			: base(parent)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BMControlCustomisationLine(Parent);
		}
	}
}
