using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(SplitDocumentForm))]
	sealed class SplitDocumentFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var document = StorageDocs.NewWithParent_DEBUG(masterFactory);
			document.SC_DataType = Core.Constants.FileFormats.TIF;
			document.SC_ImageData = FileForSplitTestPdfBytes;
			document.SC_DocType = "ACV";

			return new SplitDocumentForm(new DocumentSplitManager(document, document.ParentMain.eDocs));
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		#endregion

		DocumentFactory MasterFactory;

		public void TestSplitDocument_MessageIsShownWhenThereAreValidationErrors()
		{
			using (var form = (SplitDocumentForm)GetFormToBashCore())
			{
				var splitManager = form.BusinessEntity as DocumentSplitManager;

				AssertNotNull("SplitManager should not be null", splitManager);

				splitManager.DocumentToSplit.SC_FileName = "Test";
				splitManager.MaxSizeInMb = 2147483648m;

				var splitInfo = splitManager.DocumentSplitConfigCollection.AddNew();
				splitInfo.DocumentName = "Test";
				splitInfo.StartPage = 1;
				splitInfo.EndPage = 2;
				splitInfo.AppendPageNumber = false;

				var splitButton = form.Controls.Find("SplitButton", searchAllChildren: true).FirstOrDefault() as ZButton;

				AssertNotNull("Split button should not be null", splitButton);

				form.Show();
				splitButton.PerformClick();

				AssertEquals("Please fix validation errors before splitting.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPublishedFlagCarriedFromDocType1()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_FileName = "dummy";
			doc.SC_ImageData = new ZBlob(FileForSplitTestPdfBytes);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SC_DocType = "COT"; // published by default
			doc.SC_IsPublished = false;
			var splitManager = new DocumentSplitManager(doc);
			var splitInfoCollection = splitManager.DocumentSplitConfigCollection;
			var splitInfo = splitInfoCollection.AddNew();
			using (splitInfo.GetValidationSuspender())
			{
				splitInfo.DocumentName = "Test";
				splitInfo.DocumentType = "ACV"; // not published by default
				Assert("\"IsPublished\" property should be carried from parent's DocType", splitInfo.IsPublished);
			}
		}

		public void TestPublishedFlagCarriedFromDocType2()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_FileName = "dummy";
			doc.SC_ImageData = new ZBlob(FileForSplitTestPdfBytes);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SC_DocType = "ACV"; // not published by default
			doc.SC_IsPublished = true;
			var splitManager = new DocumentSplitManager(doc);
			var splitInfoCollection = splitManager.DocumentSplitConfigCollection;
			var splitInfo = splitInfoCollection.AddNew();
			using (splitInfo.GetValidationSuspender())
			{
				splitInfo.DocumentName = "Test";
				splitInfo.DocumentType = "COT"; // published by default
				Assert("\"IsPublished\" property should be carried from parent's DocType", !splitInfo.IsPublished);
			}
		}

		public void TestPublishedFlagWhenDocTypeIsNull1()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_FileName = "dummy";
			doc.SC_ImageData = new ZBlob(FileForSplitTestPdfBytes);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SC_DocType = "ццц"; // doesn't exist, so DocType should be set to null
			doc.SC_IsPublished = false;
			var splitManager = new DocumentSplitManager(doc);
			var splitInfoCollection = splitManager.DocumentSplitConfigCollection;
			var splitInfo = splitInfoCollection.AddNew();
			using (splitInfo.GetValidationSuspender())
			{
				splitInfo.DocumentName = "Test";
				splitInfo.DocumentType = "ййй"; // doesn't exist, so DocType should be set to null
				Assert("\"IsPublished\" property should be carried from parent", !splitInfo.IsPublished);
			}
		}

		public void TestPublishedFlagWhenDocTypeIsNull2()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_FileName = "dummy";
			doc.SC_ImageData = new ZBlob(FileForSplitTestPdfBytes);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			doc.SC_DocType = "ццц"; // doesn't exist, so DocType should be set to null
			doc.SC_IsPublished = true;
			var splitManager = new DocumentSplitManager(doc);
			var splitInfoCollection = splitManager.DocumentSplitConfigCollection;
			var splitInfo = splitInfoCollection.AddNew();
			using (splitInfo.GetValidationSuspender())
			{
				splitInfo.DocumentName = "Test";
				splitInfo.DocumentType = "ййй"; // doesn't exist, so DocType should be set to null
				Assert("\"IsPublished\" property should be carried from parent", splitInfo.IsPublished);
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly));

		byte[] FileForSplitTestPdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.FileForSplitTest.PDF");
	}
}
