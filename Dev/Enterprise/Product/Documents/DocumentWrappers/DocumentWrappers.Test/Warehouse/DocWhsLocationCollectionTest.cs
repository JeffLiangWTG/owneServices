using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsLocationCollection))]
	sealed class DocWhsLocationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsLocationCollection>
	{
		#region Constructors

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsLocation location = Factory.New<WhsLocation>();
			return DocWhsLocation.New(location, Factory);
		}

		protected override DocWhsLocationCollection GetCollectionToTest()
		{
			return new DocWhsLocationCollection(Factory);
		}

		#endregion
	}
}
