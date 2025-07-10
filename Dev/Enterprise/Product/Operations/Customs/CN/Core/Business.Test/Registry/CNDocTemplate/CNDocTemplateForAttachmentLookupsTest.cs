using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNDocTemplateForAttachmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganizations()
		{
			AssertType<OrgHeaderCollection>(parent.Lookups.Organizations);
		}

		public void TestAttachmentTypeList()
		{
			var testList = parent.Lookups.AttachmentTypeList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", testList, parent.Lookups.AttachmentTypeList);
				AssertEquals("Valid", true, testList.ContainsCode("00000001"));
			});
		}

		public void TestDocumentTypeList()
		{
			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "TT";
			docType.RT_Desc = "TT Desc";
			docType.RT_ReferenceType = "ALL";
			Factory.Save();

			AssertEquals(true, parent.Lookups.DocumentTypeList.Any(x => x.RT_DocType == "TT"));
		}

		public void TestDocumentTemplateList()
		{
			var template1 = Factory.New<StmTemplate>();
			template1.SO_Name = "T1";
			template1.SO_DataContext = "Context";
			template1.SO_IsSystemDefined = false;

			var template2 = Factory.New<StmTemplate>();
			template2.SO_Name = "T2";
			template2.SO_DataContext = "Context";
			template2.SO_IsSystemDefined = true;
			Factory.Save();

			parent.DataContext = "Context";
			var testList = parent.Lookups.DocumentTemplateList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", testList, parent.Lookups.DocumentTemplateList);
				AssertEquals("CodesAsString", "T1, T2(System)", testList.CodesAsString);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var collection = new CNDocTemplateForAttachmentCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			parent = collection.AddNew();
		}
		CNDocTemplateForAttachment parent;
	}
}
