using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PreviousDocumentCollection))]
	class PreviousDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new PreviousDocumentCollection(Factory.New<JobDeclaration>());
	}

	class PreviousDocumentCollectionExtensionsTest : TestCaseWithFactory
	{
		public void TestHasMRNPreviousDocuments()
		{
			var testCollection = new PreviousDocumentCollection(Factory.New<JobDeclaration>());
			CombineAssertions(() =>
			{
				AssertEquals("Empty collection", false, testCollection.HasMRNPreviousDocuments());

				var previousDoc = testCollection.AddNew();
				previousDoc.CSI_Code = "SUP";
				AssertEquals("SUP type", false, testCollection.HasMRNPreviousDocuments());

				previousDoc.CSI_Code = "MRN";
				AssertEquals("MRN type", true, testCollection.HasMRNPreviousDocuments());
			});
		}
	}
}
