using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(typeof(SubsetBusinessObjectCollection<>), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute), new Type[] { typeof(BusinessObjectCollectionView<>) })]
	public abstract class SubsetBusinessObjectCollectionTestCase<TCollection, TBusinessObject> : BusinessObjectCollectionBaseTestCase<TCollection>
			where TCollection : SubsetBusinessObjectCollection<TBusinessObject>
			where TBusinessObject : BusinessObject
	{
	}
}
