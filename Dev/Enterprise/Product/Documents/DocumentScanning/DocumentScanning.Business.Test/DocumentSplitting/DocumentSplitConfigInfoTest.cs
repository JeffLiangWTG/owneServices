using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Testing
{
	[TestedType(typeof(DocumentSplitConfigInfo))]
	sealed class DocumentSplitConfigInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsPublished_ReadOnly()
		{
			var main = MasterFactory.NewWithValidTestData<StorageMain>();
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_SM = main.PK;
			var splitManager = new DocumentSplitManager(doc);
			var splitConfigInfo = new DocumentSplitConfigInfo(splitManager, 3);

			AssertNull(splitConfigInfo.DocType);
			Assert(splitConfigInfo.IsPublished_ReadOnly);

			var documentType = MasterFactory.New<RefDocType>();
			documentType.RT_ReferenceType = "ALL";
			documentType.RT_DocType = "JNC";
			documentType.RT_Desc = "JNC Test Type";
			documentType.RT_IsPublishUpdatable = true;
			MasterFactory.Save();

			splitConfigInfo.DocumentType = documentType.RT_DocType;
			AssertNotNull(splitConfigInfo.DocType);
			Assert(!splitConfigInfo.IsPublished_ReadOnly);

			documentType.RT_IsPublishUpdatable = false;
			Assert(splitConfigInfo.IsPublished_ReadOnly);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
				doc.SC_DataType = Core.Constants.FileFormats.PDF;
				doc.SC_ImageData = new ZBlob(resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfFileHavingThreePages.pdf"));
				var splitManager = new DocumentSplitManager(doc);
				return splitManager.DocumentSplitConfigCollection.AddNew();
			}
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
