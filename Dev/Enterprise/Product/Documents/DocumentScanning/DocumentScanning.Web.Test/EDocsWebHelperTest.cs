using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.DocumentScanning.Web.Testing
{
	sealed class EDocsWebHelperTest : TestCaseWithFactory
	{
		public void TestIEDocsWebHelper()
		{
			Assert(new EDocsWebHelper() is IEDocsWebHelper);
		}

		#region Test Web Security

		[HttpContextEnabledTest]
		public void TestChecksForWebSecurity()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 2 }, "Not visible.txt", "MSC").IsPublished = true;

			docManagerInfo.Save();

			var siteUser = new WebContactWithDocumentOverride();
			siteUser.LoginForTest("ASS", "HOL", "random");
			AssertSame("PRE: SiteUser is set", siteUser, WebEnv.AppInstance.SiteUser);

			Assert("No visible edocs", !new EDocsWebHelper().HasEDocs(new List<ZGuid> { parent.PK }));
			AssertArrayEqualsByElements("No visible edocs", System.Array.Empty<IeDocBase>(), new EDocsWebHelper().GetEDocs(new List<ZGuid> { parent.PK }).ToArray());

			docManagerInfo.AddFileOrDocument(new byte[] { 2 }, "visible.txt", "ACV").IsPublished = true;
			docManagerInfo.Save();

			Assert("Should have the ACV doc", new EDocsWebHelper().HasEDocs(new List<ZGuid> { parent.PK }));
			AssertArrayEqualsByElements("Only allow the visible doc", new ZString[] { "visible.txt" }, new EDocsWebHelper().GetEDocs(new List<ZGuid> { parent.PK }).Select(doc => doc.FileName).ToArray());
		}

		class WebContactWithDocumentOverride : OrgContactWebUser
		{
			public override bool CanViewDocument(BusinessObjectFactory factory, RefDocType docType)
			{
				return docType.RT_DocType != "MSC";
			}
		}

		#endregion

		#region Tests GetEDocs

		public void TestGetEDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 1 }, "ACVPublished1.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 2 }, "ACVPublished2.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 4 }, "MSCPublished.txt", "MSC").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			var docManagerInfoRelated = ((IDocManagerSupport)related).DocManagerInfo;
			docManagerInfoRelated.AddFileOrDocument(new byte[] { 1 }, "RelatedACVPublished1.txt", "ACV").IsPublished = true;
			docManagerInfoRelated.AddFileOrDocument(new byte[] { 3 }, "RelatedACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfoRelated.AddFileOrDocument(new byte[] { 4 }, "RelatedMSCPublished.txt", "MSC").IsPublished = true;
			docManagerInfoRelated.MasterFactory.Save();

			var eDocs = new EDocsWebHelper().GetEDocs(new List<ZGuid> { parent.PK, related.PK }).OrderBy(doc => doc.FileName).ToList<IeDocBase>();
			AssertEquals(5, eDocs.Count);
			var eDoc1 = eDocs[0];
			AssertEquals("ACVPublished1.txt", eDoc1.FileName);
			AssertEquals("ACV", eDoc1.DocType);
			AssertEquals(new byte[] { 1 }, eDoc1.ImageData);

			var eDoc2 = eDocs[1];
			AssertEquals("ACVPublished2.txt", eDoc2.FileName);
			AssertEquals("ACV", eDoc2.DocType);
			AssertEquals(new byte[] { 2 }, eDoc2.ImageData);

			var eDoc3 = eDocs[2];
			AssertEquals("MSCPublished.txt", eDoc3.FileName);
			AssertEquals("MSC", eDoc3.DocType);
			AssertEquals(new byte[] { 4 }, eDoc3.ImageData);

			var eDoc4 = eDocs[3];
			AssertEquals("RelatedACVPublished1.txt", eDoc4.FileName);
			AssertEquals("ACV", eDoc4.DocType);
			AssertEquals(new byte[] { 1 }, eDoc4.ImageData);

			var eDoc5 = eDocs[4];
			AssertEquals("RelatedMSCPublished.txt", eDoc5.FileName);
			AssertEquals("MSC", eDoc5.DocType);
			AssertEquals(new byte[] { 4 }, eDoc5.ImageData);
		}

		public void TestGetEDocs_ParentHasNoEDocs()
		{
			var eDocs = new EDocsWebHelper().GetEDocs(new List<ZGuid> { parent.PK });
			AssertEquals(0, eDocs.Count());
		}

		public void TestGetEDocs_ParentHasOnlyUnpublishedEDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			var eDocs = new EDocsWebHelper().GetEDocs(new List<ZGuid> { parent.PK });
			AssertEquals(0, eDocs.Count());
		}

		public void TestGetEDocs_ParentDoesntExist()
		{
			var eDocs = new EDocsWebHelper().GetEDocs(new List<ZGuid> { ZGuid.NewZGuid() });
			AssertEquals(0, eDocs.Count());
		}

		#endregion

		#region Tests HasEDocs

		public void TestHasEDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 1 }, "ACVPublished1.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 2 }, "ACVPublished2.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 4 }, "MSCPublished.txt", "MSC").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			bool hasEDocs = new EDocsWebHelper().HasEDocs(new List<ZGuid> { parent.PK });
			Assert(hasEDocs);
		}

		public void TestHasEDocs_ParentHasNoEDocs()
		{
			bool hasEDocs = new EDocsWebHelper().HasEDocs(new List<ZGuid> { parent.PK });
			Assert(!hasEDocs);
		}

		public void TestHasEDocs_ParentHasOnlyUnpublishedEDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			bool hasEDocs = new EDocsWebHelper().HasEDocs(new List<ZGuid> { parent.PK });
			Assert(!hasEDocs);
		}

		public void TestHasEDocs_ParentDoesntExist()
		{
			bool hasEDocs = new EDocsWebHelper().HasEDocs(new List<ZGuid> { ZGuid.NewZGuid() });
			Assert(!hasEDocs);
		}

		#endregion

		#region Tests GetEDocsByDocType

		public void TestGetEDocsByDocType()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 1 }, "ACVPublished1.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 2 }, "ACVPublished2.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 4 }, "MSCPublished.txt", "MSC").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			var eDocsACV = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { parent.PK }, "ACV").OrderBy(doc => doc.FileName).ToList<IeDocBase>();
			AssertEquals(2, eDocsACV.Count);
			var eDocACV1 = eDocsACV[0];
			AssertEquals("ACVPublished1.txt", eDocACV1.FileName);
			AssertEquals("ACV", eDocACV1.DocType);
			AssertEquals(new byte[] { 1 }, eDocACV1.ImageData);

			var eDocACV2 = eDocsACV[1];
			AssertEquals("ACVPublished2.txt", eDocACV2.FileName);
			AssertEquals("ACV", eDocACV2.DocType);
			AssertEquals(new byte[] { 2 }, eDocACV2.ImageData);

			var eDocsMSC = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { parent.PK }, "MSC").ToList<IeDocBase>();
			AssertEquals(1, eDocsMSC.Count);
			var eDocMSC = eDocsMSC[0];
			AssertEquals("MSCPublished.txt", eDocMSC.FileName);
			AssertEquals("MSC", eDocMSC.DocType);
			AssertEquals(new byte[] { 4 }, eDocMSC.ImageData);

			var eDocsXXX = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { parent.PK }, "XXX");
			AssertEquals(0, eDocsXXX.Count());
		}

		public void TestGetEDocsByDocType_ParentHasNoEDocs()
		{
			var eDocs = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { parent.PK }, "ACV");
			AssertEquals(0, eDocs.Count());
		}

		public void TestGetEDocsByDocType_ParentHasOnlyUnpublishedEDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			var eDocsACV = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { parent.PK }, "ACV");
			AssertEquals(0, eDocsACV.Count());

			var eDocsMSC = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { parent.PK }, "MSC");
			AssertEquals(0, eDocsMSC.Count());

			var eDocsXXX = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { parent.PK }, "XXX");
			AssertEquals(0, eDocsXXX.Count());
		}

		public void TestGetEDocsByDocType_ParentDoesntExist()
		{
			var eDocs = new EDocsWebHelper().GetEDocsByDocType(new List<ZGuid> { ZGuid.NewZGuid() }, "ACV");
			AssertEquals(0, eDocs.Count());
		}

		#endregion

		#region Tests HasEDocsByDocType

		public void TestHasEDocsByDocType()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 1 }, "ACVPublished1.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 2 }, "ACVPublished2.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 4 }, "MSCPublished.txt", "MSC").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			bool hasEDocsACV = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { parent.PK }, "ACV");
			Assert(hasEDocsACV);

			bool hasEDocsMSC = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { parent.PK }, "MSC");
			Assert(hasEDocsMSC);

			bool hasEDocsXXX = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { parent.PK }, "XXX");
			Assert(!hasEDocsXXX);
		}

		public void TestHasEDocsByDocType_ParentHasNoEDocs()
		{
			bool hasEDocs = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { parent.PK }, "ACV");
			Assert(!hasEDocs);
		}

		public void TestHasEDocsByDocType_ParentHasOnlyUnpublishedEDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 3 }, "ACVUnpublished.txt", "ACV").IsPublished = false;
			docManagerInfo.AddFileOrDocument(new byte[] { 5 }, "MSCUnpublished.txt", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			bool hasEDocsACV = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { parent.PK }, "ACV");
			Assert(!hasEDocsACV);

			bool hasEDocsMSC = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { parent.PK }, "MSC");
			Assert(!hasEDocsMSC);

			bool hasEDocsXXX = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { parent.PK }, "XXX");
			Assert(!hasEDocsXXX);
		}

		public void TestHasEDocsByDocType_ParentDoesntExist()
		{
			bool hasEDocs = new EDocsWebHelper().HasEDocsByDocType(new List<ZGuid> { ZGuid.NewZGuid() }, "ACV");
			Assert(!hasEDocs);
		}

		#endregion

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			(new DocManagerDBHelper()).LastWritableDatabaseWithFreeSpace(); //to ensure SD001 exists
			documentFactory = new DocumentFactoryProvider().GetFactory(Factory);

			parent = documentFactory.New<OrgHeader>();
			parent.OH_Code = "UnitTest";

			storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = 1;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			storageMain.SM_ParentFK = parent.PK;

			related = documentFactory.New<OrgHeader>();
			related.OH_Code = "Related";

			storageMain2 = documentFactory.New<StorageMain>();
			storageMain2.SM_DB = 1;
			storageMain2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			storageMain2.SM_ParentFK = related.PK;

			documentFactory.Save();
		}

		DocumentFactory documentFactory;
		OrgHeader parent;
		StorageMain storageMain;

		OrgHeader related;
		StorageMain storageMain2;

		#endregion
	}
}
