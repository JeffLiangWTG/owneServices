using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.DataTransfer.Universal;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	public class DocumentFilterRelatedEDocTest : TestCaseWithFactory
	{
		public void TestAddFilterValue()
		{
			var filter = new DocumentFilterRelatedEDoc();
			Assert(filter.Values.Count == 0);

			filter.AddFilterValue(" ");
			Assert(filter.Values.Count == 0);

			filter.AddFilterValue("ABC 123");
			AssertEquals("ABC 123", filter.Values[0]);

			filter.AddFilterValue("ABC 123");
			AssertEquals(1, filter.Values.Count);

			filter.AddFilterValue("ABC 123*");
			AssertEquals(2, filter.Values.Count);
		}

		public void TestIsMatch()
		{
			var filter_Empty = new DocumentFilterRelatedEDoc();
			filter_Empty.AddFilterValue("");
			var filter_NoStarSign = new DocumentFilterRelatedEDoc();
			filter_NoStarSign.AddFilterValue("AB");
			var filter_OnlyStarSign = new DocumentFilterRelatedEDoc();
			filter_OnlyStarSign.AddFilterValue("*");
			var filter_EndWithStarSign = new DocumentFilterRelatedEDoc();
			filter_EndWithStarSign.AddFilterValue("C*");
			var filter_HasStarSin = new DocumentFilterRelatedEDoc();
			filter_HasStarSin.AddFilterValue("*D");
			var filter_MultipleValues = new DocumentFilterRelatedEDoc();
			filter_MultipleValues.AddFilterValue("AB");
			filter_MultipleValues.AddFilterValue("C*");

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var storageMainForTest = documentFactory.New<StorageMainForTest>();
			storageMainForTest.SetDocumentOwnerDescription("AB");
			var eDoc_AB = new eDocForTest();
			eDoc_AB.ParentMain = storageMainForTest;

			storageMainForTest = documentFactory.New<StorageMainForTest>();
			storageMainForTest.SetDocumentOwnerDescription("ABC");
			var eDoc_ABC = new eDocForTest();
			eDoc_ABC.ParentMain = storageMainForTest;

			storageMainForTest = documentFactory.New<StorageMainForTest>();
			storageMainForTest.SetDocumentOwnerDescription("CD");
			var eDoc_CD = new eDocForTest();
			eDoc_CD.ParentMain = storageMainForTest;

			storageMainForTest = documentFactory.New<StorageMainForTest>();
			storageMainForTest.SetDocumentOwnerDescription("CE");
			var eDoc_CE = new eDocForTest();
			eDoc_CE.ParentMain = storageMainForTest;

			storageMainForTest = documentFactory.New<StorageMainForTest>();
			storageMainForTest.SetDocumentOwnerDescription("C*");
			var eDoc_CStarSign = new eDocForTest();
			eDoc_CStarSign.ParentMain = storageMainForTest;

			AssertEquals("All eDocs should be matched by the empty values", true, filter_Empty.IsMatch(eDoc_AB));
			AssertEquals("All eDocs should be matched by the empty values", true, filter_Empty.IsMatch(eDoc_ABC));
			AssertEquals("All eDocs should be matched by the empty values", true, filter_Empty.IsMatch(eDoc_CD));
			AssertEquals("All eDocs should be matched by the empty values", true, filter_Empty.IsMatch(eDoc_CE));
			AssertEquals("All eDocs should be matched by the empty values", true, filter_Empty.IsMatch(eDoc_CStarSign));

			AssertEquals("Only AB is matched", true, filter_NoStarSign.IsMatch(eDoc_AB));
			AssertEquals("Only AB is matched", false, filter_NoStarSign.IsMatch(eDoc_ABC));
			AssertEquals("Only AB is matched", false, filter_NoStarSign.IsMatch(eDoc_CD));
			AssertEquals("Only AB is matched", false, filter_NoStarSign.IsMatch(eDoc_CE));
			AssertEquals("Only AB is matched", false, filter_NoStarSign.IsMatch(eDoc_CStarSign));

			AssertEquals("All eDocs should be matched by the *", true, filter_OnlyStarSign.IsMatch(eDoc_AB));
			AssertEquals("All eDocs should be matched by the *", true, filter_OnlyStarSign.IsMatch(eDoc_ABC));
			AssertEquals("All eDocs should be matched by the *", true, filter_OnlyStarSign.IsMatch(eDoc_CD));
			AssertEquals("All eDocs should be matched by the *", true, filter_OnlyStarSign.IsMatch(eDoc_CE));
			AssertEquals("All eDocs should be matched by the *", true, filter_OnlyStarSign.IsMatch(eDoc_CStarSign));

			AssertEquals("eDoc starting with C will be matched", false, filter_EndWithStarSign.IsMatch(eDoc_AB));
			AssertEquals("eDoc starting with C will be matched", false, filter_EndWithStarSign.IsMatch(eDoc_ABC));
			AssertEquals("eDoc starting with C will be matched", true, filter_EndWithStarSign.IsMatch(eDoc_CD));
			AssertEquals("eDoc starting with C will be matched", true, filter_EndWithStarSign.IsMatch(eDoc_CE));
			AssertEquals("eDoc starting with C will be matched", true, filter_EndWithStarSign.IsMatch(eDoc_CStarSign));

			AssertEquals("No eDoc should match *D", false, filter_HasStarSin.IsMatch(eDoc_AB));
			AssertEquals("No eDoc should match *D", false, filter_HasStarSin.IsMatch(eDoc_ABC));
			AssertEquals("No eDoc should match *D", false, filter_HasStarSin.IsMatch(eDoc_CD));
			AssertEquals("No eDoc should match *D", false, filter_HasStarSin.IsMatch(eDoc_CE));
			AssertEquals("No eDoc should match *D", false, filter_HasStarSin.IsMatch(eDoc_CStarSign));

			AssertEquals("AB and starting with C will be matched", true, filter_MultipleValues.IsMatch(eDoc_AB));
			AssertEquals("AB and starting with C will be matched", false, filter_MultipleValues.IsMatch(eDoc_ABC));
			AssertEquals("AB and starting with C will be matched", true, filter_MultipleValues.IsMatch(eDoc_CD));
			AssertEquals("AB and starting with C will be matched", true, filter_MultipleValues.IsMatch(eDoc_CE));
			AssertEquals("AB and starting with C will be matched", true, filter_MultipleValues.IsMatch(eDoc_CStarSign));
		}
	}
}
