using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.Testing
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
