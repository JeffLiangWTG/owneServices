using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsWorkOrderLineCollection))]
	sealed class DocWhsWorkOrderLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsWorkOrderLineCollection>
	{
		#region Constructors

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var whsWorkOrderLine = Factory.New<WhsWorkOrderLine>();
			return DocWhsWorkOrderLine.New(whsWorkOrderLine, Factory);
		}

		protected override DocWhsWorkOrderLineCollection GetCollectionToTest()
		{
			return new DocWhsWorkOrderLineCollection(Factory);
		}

		#endregion
	}
}
