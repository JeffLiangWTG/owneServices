using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsReceiveLineCollection))]
	sealed class DocWhsReceiveLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsReceiveLineCollection>
	{
		#region Constructors

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var whsReceiveLine = Factory.New<WhsReceiveLine>();
			return DocWhsReceiveLine.New(whsReceiveLine, Factory);
		}

		protected override DocWhsReceiveLineCollection GetCollectionToTest()
		{
			return new DocWhsReceiveLineCollection(Factory);
		}

		#endregion
	}
}
