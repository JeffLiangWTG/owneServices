using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsAdjustmentLineCollection))]
	sealed class DocWhsAdjustmentLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsAdjustmentLineCollection>
	{
		#region Constructors

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var whsAdjustmentLine = Factory.New<WhsAdjustmentLine>();
			return DocWhsAdjustmentLine.New(whsAdjustmentLine, Factory);
		}

		protected override DocWhsAdjustmentLineCollection GetCollectionToTest()
		{
			return new DocWhsAdjustmentLineCollection(Factory);
		}

		#endregion
	}
}
