using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(LoadListPackLineCollection))]
	internal class LoadListPackLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LoadListPackLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LoadListPackLine(null, null);
		}

		protected override LoadListPackLineCollection GetCollectionToTest()
		{
			return new LoadListPackLineCollection(Factory);
		}
	}
}
