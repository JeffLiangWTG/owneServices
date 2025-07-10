using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocCusEntryHeaderCollection))]
	sealed class DocCusEntryHeaderCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryHeaderCollection>
	{
		protected override DocCusEntryHeaderCollection GetCollectionToTest() => new DocCusEntryHeaderCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			return DocCusEntryHeader.New(cusEntryHeader, Factory);
		}
	}
}
