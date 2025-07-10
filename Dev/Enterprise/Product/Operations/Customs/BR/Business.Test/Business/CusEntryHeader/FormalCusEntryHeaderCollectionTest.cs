using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(FormalCusEntryHeaderCollection))]
	class FormalCusEntryHeaderCollectionTest : BusinessObjectCollectionViewTestCase<FormalCusEntryHeaderCollection>
	{
		public void TestIsThisPartOfTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = MessageTypeList.Codes.SUF;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.CDI;
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = MessageTypeList.Codes.CDD;
			AssertContainsExactElementsInAnyOrder(new[] { entryHeader2, entryHeader3 }, new FormalCusEntryHeaderCollection(declaration));
		}

		protected override FormalCusEntryHeaderCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var collection = new FormalCusEntryHeaderCollection(declaration);
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => GetCollectionToTest().AddNew();
	}
}
