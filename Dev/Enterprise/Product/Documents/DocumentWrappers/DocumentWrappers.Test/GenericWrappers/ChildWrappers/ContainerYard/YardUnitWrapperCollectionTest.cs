using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(YardUnitWrapperCollection))]
	sealed class YardUnitWrapperCollectionTest : GenericWrapperCollectionTest<YardUnitWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return GetNewYardUnitWrapper();
		}

		protected override YardUnitWrapperCollection GetNewDocumentWrapperCollection()
		{
			var collection = new YardUnitWrapperCollection(Factory);
			collection.Add(GetNewYardUnitWrapper());
			return collection;
		}

		YardUnitWrapper GetNewYardUnitWrapper()
		{
			var yardUnitState = Factory.New<CYDYardUnitState>();
			return new YardUnitWrapper(yardUnitState, Factory);
		}
	}
}
