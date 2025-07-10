using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyNotesWithIBusinessObjectCollectionWithMaster : Notes
	{
		public DummyNotesWithIBusinessObjectCollectionWithMaster(IStmNoteParent parent) : base(parent)
		{ }

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			if (Parent is BusinessObject businessObject)
			{
				return new DummyIBusinessObjectCollectionWithMaster(businessObject, Factory);
			}
			return null;
		}
	}
}
