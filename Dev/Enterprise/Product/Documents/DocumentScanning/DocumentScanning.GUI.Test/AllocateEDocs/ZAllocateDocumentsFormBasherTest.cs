using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(ZAllocateDocumentsForm))]
	sealed class ZAllocateDocumentsFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			SetupSavedTestData();
			return new ZAllocateDocumentsForm(new AllocateDocumentsManager(new DocumentFactoryProvider().GetFactory(Factory)));
		}

		void SetupSavedTestData()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly))
			{
				var testImage = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

				DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(Factory);

				ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
				OrgHeader org1 = masterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

				filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
				OrgHeader org2 = masterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

				filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C");
				OrgHeader org3 = masterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

				filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D");
				OrgHeader org4 = masterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

				filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "E");
				OrgHeader org5 = masterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

				StorageMain parentFirstSet = masterFactory.New(typeof(StorageMain)) as StorageMain;
				masterFactory.AllocateToDB(parentFirstSet);
				parentFirstSet.SM_ParentFK = org1.PK;

				StorageDocs doc1FirstSet = StorageDocs.NewWithParent_DEBUG(masterFactory);
				doc1FirstSet.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc1FirstSet.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc1FirstSet.SC_Desc = "AAA";
				doc1FirstSet.SC_ImageData = testImage;

				StorageDocs doc2FirstSet = StorageDocs.NewWithParent_DEBUG(masterFactory);
				doc2FirstSet.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc2FirstSet.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc2FirstSet.SC_Desc = "AAA";
				doc2FirstSet.SC_ImageData = testImage;

				StorageDocs doc3FirstSet = StorageDocs.NewWithParent_DEBUG(masterFactory);
				doc3FirstSet.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc3FirstSet.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc3FirstSet.SC_Desc = "AAA";
				doc3FirstSet.SC_ImageData = testImage;

				StorageMain parentSecondSet = masterFactory.New(typeof(StorageMain)) as StorageMain;
				masterFactory.AllocateToDB(parentSecondSet);
				parentSecondSet.SM_ParentFK = org2.PK;

				StorageDocs doc1SecondSet = StorageDocs.NewWithParent_DEBUG(masterFactory);
				doc1SecondSet.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc1SecondSet.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc1SecondSet.SC_Desc = "AAA";
				doc1SecondSet.SC_ImageData = testImage;

				StorageDocs doc2SecondSet = StorageDocs.NewWithParent_DEBUG(masterFactory);
				doc2SecondSet.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc2SecondSet.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc2SecondSet.SC_Desc = "AAA";
				doc2SecondSet.SC_ImageData = testImage;

				StorageDocs doc3SecondSet = StorageDocs.NewWithParent_DEBUG(masterFactory);
				doc3SecondSet.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc3SecondSet.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc3SecondSet.SC_Desc = "AAA";
				doc3SecondSet.SC_ImageData = testImage;

				AllocateDocumentsManager manager1 = new AllocateDocumentsManager(masterFactory);
				StorageDocsUnallocated doc1UnallocatedSet1 = manager1.UnallocatedDocuments.AddNew();
				doc1UnallocatedSet1.SC_ImageData = testImage;

				StorageDocsUnallocated doc1UnallocatedSet2 = manager1.UnallocatedDocuments.AddNew();
				doc1UnallocatedSet2.SC_ImageData = testImage;

				StorageDocsUnallocated doc1UnallocatedSet3 = manager1.UnallocatedDocuments.AddNew();
				doc1UnallocatedSet3.SC_ImageData = testImage;

				StorageDocsUnallocated doc1UnallocatedSet4 = manager1.UnallocatedDocuments.AddNew();
				doc1UnallocatedSet4.SC_ImageData = testImage;

				NumberedBusinessObjectFactory firstSetFactory = masterFactory.GetFactory(parentFirstSet.SM_DB);
				NumberedBusinessObjectFactory secondSetFactory = masterFactory.GetFactory(parentSecondSet.SM_DB);

				masterFactory.Save();
			}
		}
	}
}
