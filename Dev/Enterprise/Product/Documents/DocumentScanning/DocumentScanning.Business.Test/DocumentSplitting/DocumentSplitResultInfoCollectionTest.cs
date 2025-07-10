using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(DocumentSplitResultInfoCollection))]
	sealed class DocumentSplitResultInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSplitResultInfoCollection>
	{
		public void TestLoad()
		{
			var testCollection = new DocumentSplitResultInfoCollection(null);
			AssertEquals("Initial count", 0, testCollection.Count);
			AssertEquals("Has changes?", false, testCollection.HasChanges);
		}

		#region Implementation

		protected override DocumentSplitResultInfoCollection GetCollectionToTest()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			var splitManager = new DocumentSplitManager(doc);
			return splitManager.DocumentSplitResultCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var splitManager = new DocumentSplitManager(doc);
			return new DocumentSplitResultInfo(splitManager);
		}

		public void TestRetreivedDescriptionType_SplitResult()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SM_Type = "SHP";
			doc.SC_ImageData = new ZBlob(PdfFileHavingThreePages);
			var splitManager = new DocumentSplitManager(doc);

			var testCollection = new DocumentSplitResultInfoCollection(splitManager);
			var resultInfo = testCollection.AddNew();

			resultInfo.DocumentType = Core.Constants.RefDocTypes.AgentsInvoice;
			AssertEquals("AGI", resultInfo.DocumentType);
			AssertEquals("Agents Invoice", resultInfo.DescriptionType);
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

			var testCollection = new DocumentSplitResultInfoCollection(splitManager);
			var resultInfo = testCollection.AddNew();

			resultInfo.DocumentType = Core.Constants.RefDocTypes.AgentsInvoice;
			AssertEquals("AGI", resultInfo.DocumentType);
			AssertEquals("Agents Invoice", resultInfo.DescriptionType);

			resultInfo.DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertEquals("MSC", resultInfo.DocumentType);
			AssertEquals("Sango Test", resultInfo.DescriptionType);
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
