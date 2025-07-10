using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.DocTransactionHeader;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(FlattenedLineCollection))]
	sealed class DocTransactionHeaderFlattenedLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FlattenedLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return FlattenedLine.New(DocTransactionHeader.New(Factory.New<APInvoice>(), Factory), Factory);
		}

		protected override FlattenedLineCollection GetCollectionToTest()
		{
			return new FlattenedLineCollection(new DocTransactionHeaderCollection(Factory));
		}
	}
}
