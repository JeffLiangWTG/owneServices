using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(EDocCollectionView))]
	sealed class EDocCollectionViewTest : BusinessObjectCollectionViewTestCase<EDocCollectionView>
	{
		public void TestRebuild()
		{
			var docPack = new DocumentPack();
			var dumy = Factory.New<DocumentPrintSetTest.DummyBizoStorageDocs>();
			docPack.Add(dumy);

			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName)));
				docPack.Add(report);
			}

			AssertEquals(2, docPack.Count);
			var edocsView = new EDocCollectionView(docPack, new DocDeliveryContactCollection(Factory));
			AssertEquals(1, edocsView.Count);
		}

		public void TestResetEDocs()
		{
			var docPack = new DocumentPack();
			var dumy = Factory.New<DocumentPrintSetTest.DummyBizoStorageDocs>();
			docPack.Add(dumy);
			Assert("The value of the IncludedInPrint should be false", !dumy.IncludedInPrint);
			Assert("The value of the ShouldPrintByDefault should be false", !dumy.ShouldPrintByDefault);

			dumy.IncludedInPrint = true;
			dumy.ShouldPrintByDefault = true;
			Assert("The value of the IncludedInPrint should be true", dumy.IncludedInPrint);
			Assert("The value of the ShouldPrintByDefault should be true", dumy.IncludedInPrint);

			var edocsView = new EDocCollectionView(docPack, new DocDeliveryContactCollection(Factory));
			edocsView.ResetEDocs();
			Assert("The value of the IncludedInPrint should be reset to true", dumy.IncludedInPrint);
			Assert("The value of the ShouldPrintByDefault should be reset to false", !dumy.ShouldPrintByDefault);
		}

		protected override EDocCollectionView GetCollectionToTest() => new EDocCollectionView(new DocumentPack(), new DocDeliveryContactCollection(Factory));

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<DocumentPrintSetTest.DummyBizoStorageDocs>();
	}
}
