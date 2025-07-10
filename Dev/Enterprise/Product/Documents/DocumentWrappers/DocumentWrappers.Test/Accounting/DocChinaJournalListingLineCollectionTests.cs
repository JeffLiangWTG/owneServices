using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.DataInterface;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocChinaJournalListingLineCollection))]
	public class DocChinaJournalListingLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocChinaJournalListingLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var voucherLine = new ChinaJournal();
			return DocChinaJournalListingLine.New(voucherLine, Factory);
		}

		protected override DocChinaJournalListingLineCollection GetCollectionToTest()
		{
			return new DocChinaJournalListingLineCollection(Factory);
		}
	}
}
