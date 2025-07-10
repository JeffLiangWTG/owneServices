using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocManagerInfoForTest : DocManagerInfo
	{
		public DocManagerInfoForTest(BusinessObject parent)
			: base(parent, Core.Constants.DocManagerCodes.UNLOCO)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New(typeof(DummyDocManagerTestBizO));

			EDocsTestHelper.CreateDocType(factory, "FFF", "UNL", "F Test Document FFF");

			var documentFactory = EDocsTestHelper.GetDocumentFactory(factory);

			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, dummy);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "FFF", false).Description = "F Test Document FFF";
			}

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgStorageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, orgHeader);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				orgStorageMain.AddFileOrDocument(contents, string.Empty, "FFF", false).Description = "F Test Document FFF for OrgHeader";
			}

			documentFactory.Save();
			factory.Save();

			var relatedBusinessObjects = new List<BusinessObject>();
			relatedBusinessObjects.Add(dummy);
			relatedBusinessObjects.Add(orgHeader);

			if (BusinessEntity is DummyDocManagerTestBizO dummyDocManagerTestBizO && dummyDocManagerTestBizO.AddNonIDocumentSupportableObjectForTest)
			{
				relatedBusinessObjects.Add(factory.NewWithValidTestData<DummyBusinessObject>());
			}

			return relatedBusinessObjects.ToArray();
		}
	}
}
