using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocCusEntryHeaderCollection))]
sealed class DocCusEntryHeaderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryHeaderCollection>
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return DocBaseCusEntryHeader.New(Factory.New<CusEntryHeader>(), Factory);
	}

	protected override DocCusEntryHeaderCollection GetCollectionToTest()
	{
		return new DocCusEntryHeaderCollection(Factory);
	}
}
