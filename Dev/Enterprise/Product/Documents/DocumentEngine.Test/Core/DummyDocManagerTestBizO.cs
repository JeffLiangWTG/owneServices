using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	public class DummyDocManagerTestBizO : RefUNLOCO, IDocumentSupportable, IDocManagerSupport
	{
		public DummyDocManagerTestBizO(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public void SetupDocManagerObjects()
		{
			var factory = new BusinessObjectFactory();

			EDocsTestHelper.CreateDocType(factory, "AAA", "UNL", "A Test Document AAA");
			EDocsTestHelper.CreateDocType(factory, "BBB", "UNL", "A Test Document BBB");
			EDocsTestHelper.CreateDocType(factory, "CCC", "UNL", "A Test Document CCC");

			var documentFactory = EDocsTestHelper.GetDocumentFactory(factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, this);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = "A Test Document AAA";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "BBB", false).Description = "A Test Document BBB";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "CCC", false).Description = "A Test Document CCC";
			}

			documentFactory.Save();
			factory.Save();
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				return GetDocManagerInfoForTest();
			}
		}

		public virtual DocManagerInfo GetDocManagerInfoForTest()
		{
			if (docManagerInfo == null)
			{
				docManagerInfo = AddRelatedObjectsForTest ? new DocManagerInfoForTest(this) : new DocManagerInfo(this, Core.Constants.DocManagerCodes.UNLOCO);
			}
			return docManagerInfo;
		}

		DocManagerInfo docManagerInfo;
		public bool AddRelatedObjectsForTest;
		public bool AddNonIDocumentSupportableObjectForTest;

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DummyDocManagerTestBizODocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;
	}
}
