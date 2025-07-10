using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CIQRequiredDocumentCollection))]
	class CIQRequiredDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var testCollection = GetCollectionToTest();
			var testItem = (CIQRequiredDocument)testCollection.AddNew();
			AssertEquals(1, testItem.XC_NumberOfOriginals);
			AssertEquals(2, testItem.XC_NumberOfCopies);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().CIQRequiredDocuments;
	}
}
