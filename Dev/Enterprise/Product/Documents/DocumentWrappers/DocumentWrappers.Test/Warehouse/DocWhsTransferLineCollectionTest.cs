using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsTransferLineCollection))]
	sealed class DocWhsTransferLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsTransferLineCollection>
	{
		#region Constructors

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var whsTransferLine = Factory.New<WhsTransferLine>();
			return DocWhsTransferLine.New(whsTransferLine, Factory);
		}

		protected override DocWhsTransferLineCollection GetCollectionToTest()
		{
			return new DocWhsTransferLineCollection(Factory);
		}

		#endregion
	}
}
