using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.DocTransactionHeader;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(FilteredFlattenedLineCollection))]
	sealed class FilteredFlattenedLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FilteredFlattenedLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return FlattenedLine.New(DocTransactionHeader.New(Factory.New<APInvoice>(), Factory), Factory);
		}

		protected override FilteredFlattenedLineCollection GetCollectionToTest()
		{
			return new FilteredFlattenedLineCollection(new DocTransactionHeaderCollection(Factory));
		}
	}
}
