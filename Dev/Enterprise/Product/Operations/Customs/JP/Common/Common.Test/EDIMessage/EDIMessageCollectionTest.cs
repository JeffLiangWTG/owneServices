using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	sealed class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EDIMessage>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIMessageCollection(EntryHeader);
		}

		[ExpectNoExceptions]
		public void TestEntryHeaderMessageCollection()
		{
			AssertEquals(EntryHeader.Messages.GetType(), typeof(EDIMessageCollection));
		}

		BaseJobDeclaration Declaration => declaration ?? (declaration = Factory.New<BaseJobDeclaration>());
		BaseJobDeclaration declaration;

		CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = Declaration.CustomsEntryHeaders.AddNew());
		CusEntryHeader entryHeader;
	}
}
