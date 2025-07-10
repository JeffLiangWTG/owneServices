using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Testing
{
	class ActiveOrAllBusinessObjectCollection : DummyBusinessObjectCollectionView
	{
		public ActiveOrAllBusinessObjectCollection(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var dummy = (DummyBusinessObject)element;
			return dummy.Z0_Bool;
		}
	}
}
