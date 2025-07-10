using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocCusEntryHeaderCollection))]
	sealed class DocCusEntryHeaderCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			return DocCusEntryHeader.New(cusEntryHeader, Factory);
		}

		protected override DocCusEntryHeaderCollection GetCollectionToTest()
		{
			return new DocCusEntryHeaderCollection(Factory);
		}
	}
}
