using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Testing
{
	[TestedType(typeof(DocumentSplitConfigInfoCollection))]
	sealed class DocumentSplitConfigInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSplitConfigInfoCollection>
	{
		public void TestLoad()
		{
			var testCollection = new DocumentSplitConfigInfoCollection(GetSplitManager(), 3);
			AssertEquals("Initial count", 0, testCollection.Count);
			AssertEquals("Has changes?", false, testCollection.HasChanges);
		}

		public void TestHasNotSplitElements()
		{
			var testCollection = new DocumentSplitConfigInfoCollection(GetSplitManager(), 3);
			Assert("There's no split config needs to be split", !testCollection.HasNotSplitElements);
			var item = testCollection.AddNew();
			item.NeedsSplitting = true;
			Assert("There are some split config(s) need to be split", testCollection.HasNotSplitElements);
		}

		public void TestDefaultValueOfNewRecord()
		{
			var testCollection = new DocumentSplitConfigInfoCollection(GetSplitManager(), 3);
			var splitInfo = testCollection.AddNew();
			AssertEquals(1, splitInfo.StartPage);
			AssertEquals(3, splitInfo.EndPage);

			splitInfo.EndPage = 2;

			var splitInfo1 = testCollection.AddNew();
			AssertEquals(3, splitInfo1.StartPage);
			AssertEquals(3, splitInfo1.EndPage);
		}
		public void TestRetreivedDescriptionType()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SM_Type = "SHP";
			doc.SC_ImageData = new ZBlob(PdfFileHavingThreePages);
			var splitManager = new DocumentSplitManager(doc);

			var testCollection = new DocumentSplitConfigInfoCollection(splitManager, 3);
			var splitInfo = testCollection.AddNew();

			splitInfo.DocumentType = Core.Constants.RefDocTypes.AgentsInvoice;
			AssertEquals("AGI", splitInfo.DocumentType);
			AssertEquals("Agents Invoice", splitInfo.DescriptionType);

			var splitInfo1 = testCollection.AddNew();
			splitInfo1.DocumentType = "ATD";
			AssertEquals("Authority to Deal", splitInfo1.DescriptionType);

			splitInfo.DocumentType = "BDR";
			AssertEquals("Bank Draft", splitInfo.DescriptionType);
		}

		public void TestRetreivedDescription()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SM_Type = "SHP";
			doc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			doc.SC_Desc = "Sango Test";
			doc.SC_ImageData = new ZBlob(PdfFileHavingThreePages);
			var splitManager = new DocumentSplitManager(doc);

			var testCollection = new DocumentSplitConfigInfoCollection(splitManager, 3);
			var splitInfo = testCollection.AddNew();

			splitInfo.DocumentType = Core.Constants.RefDocTypes.AgentsInvoice;
			AssertEquals("AGI", splitInfo.DocumentType);
			AssertEquals(Core.Constants.RefDocTypeDescriptions.AgentsInvoice, splitInfo.DescriptionType);

			var splitInfo1 = testCollection.AddNew();
			splitInfo1.DocumentType = Core.Constants.RefDocTypes.AuthorityToDeal;
			AssertEquals(Core.Constants.RefDocTypeDescriptions.AuthorityToDeal, splitInfo1.DescriptionType);

			splitInfo1.DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertEquals("Sango Test", splitInfo1.DescriptionType);
		}

		#region Implementation

		protected override DocumentSplitConfigInfoCollection GetCollectionToTest()
		{
			return GetSplitManager().DocumentSplitConfigCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var splitManager = new DocumentSplitManager(doc);
			return new DocumentSplitConfigInfo(splitManager, 3);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] PdfFileHavingThreePages => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfFileHavingThreePages.pdf");

		DocumentSplitManager GetSplitManager()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SC_ImageData = new ZBlob(PdfFileHavingThreePages);
			var splitManager = new DocumentSplitManager(doc);
			return splitManager;
		}

		DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		#endregion
	}
}
