using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class NumberedBusinessObjectFactoryForParentFactoryTest : NumberedBusinessObjectFactoryForParentFactoryBaseTest
	{
		protected override NumberedBusinessObjectFactory GetFactory(BusinessObjectFactory factory)
		{
			return new NumberedBusinessObjectFactory(0, new DocumentFactoryProvider().GetFactory(factory));
		}
	}

	public abstract class NumberedBusinessObjectFactoryForParentFactoryBaseTest : TestCaseWithFactory
	{
		#region Create Random Objects

		DocumentFactory docFactory;
		DocumentFactory DocFactory
		{
			get
			{
				if (docFactory == null)
				{
					docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return docFactory;
			}
		}

		void SaveFactories()
		{
			DocFactory.Save();
			Factory.Save();
		}

		KeyValuePair<ZGuid, IZType> CreateRandomOrg()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = Guid.NewGuid().ToString().Replace("-", "").Substring(0, OrgHeaderSchema.OH_Code.MaxLength);
			org.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			return new KeyValuePair<ZGuid, IZType>(org.PK, org.OH_Code);
		}

		KeyValuePair<ZGuid, IZType> CreateRandomStorageDoc()
		{
			StorageDocs doc = DocFactory.New<StorageDocs>();
			doc.SC_Desc = GetRandomString(StorageDocsSchema.SC_Desc.MaxLength);
			doc.SC_SM = CreateRandomStorageMain().Key;
			return new KeyValuePair<ZGuid, IZType>(doc.PK, doc.SC_Desc);
		}

		KeyValuePair<ZGuid, IZType> CreateRandomStorageDocUnallocated()
		{
			StorageDocsUnallocated doc = DocFactory.New<StorageDocsUnallocated>();
			doc.SC_Desc = GetRandomString(StorageDocsSchema.SC_Desc.MaxLength);
			return new KeyValuePair<ZGuid, IZType>(doc.PK, doc.SC_Desc);
		}

		KeyValuePair<ZGuid, IZType> CreateRandomStorageFile()
		{
			StorageFile doc = DocFactory.New<StorageFile>();
			doc.SC_Desc = GetRandomString(StorageDocsSchema.SC_Desc.MaxLength);
			return new KeyValuePair<ZGuid, IZType>(doc.PK, doc.SC_Desc);
		}

		protected KeyValuePair<ZGuid, IZType> CreateRandomStorageMain()
		{
			DocumentFactory fac = DocFactory ?? DocFactory.MasterFactory;
			StorageMain main = fac.New<StorageMain>();
			main.SM_PhysicalLocation = GetRandomString(StorageMainSchema.SM_PhysicalLocation.MaxLength);
			main.SM_ParentFK = CreateRandomOrg().Key;
			main.SM_DB = 1;
			return new KeyValuePair<ZGuid, IZType>(main.PK, main.SM_ParentFK);
		}

		KeyValuePair<ZGuid, IZType> CreateRandomJobRequiredDocument()
		{
			JobRequiredDocument doc = Factory.New<JobRequiredDocument>();
			doc.EQ_DocDescription = GetRandomString(JobRequiredDocumentSchema.EQ_DocDescription.MaxLength);
			return new KeyValuePair<ZGuid, IZType>(doc.PK, doc.EQ_DocDescription);
		}

		KeyValuePair<ZGuid, IZType> CreateRandomRefDocType()
		{
			RefDocType doc = Factory.New<RefDocType>();
			doc.RT_DocType = GetRandomString(RefDocTypeSchema.RT_DocType.MaxLength);
			doc.RT_ReferenceType = "ALL";
			return new KeyValuePair<ZGuid, IZType>(doc.PK, doc.RT_Desc);
		}

		KeyValuePair<ZGuid, IZType> CreateRandomUser()
		{
			GlbStaff user = Factory.New<GlbStaff>();
			user.GS_Code = GetRandomString(GlbStaffSchema.GS_Code.MaxLength);
			return new KeyValuePair<ZGuid, IZType>(user.PK, user.GS_Code);
		}

		string GetRandomString(int length)
		{
			return new ZString(Guid.NewGuid().ToString().Replace("-", "")).Left(length);
		}

		#endregion

		public delegate KeyValuePair<ZGuid, IZType> CreateRandomObject();

		protected abstract NumberedBusinessObjectFactory GetFactory(BusinessObjectFactory factory);

		public virtual void TestOnlyAllowedObjectsAreInFactory_LoadArray()
		{
			AssertOnlyAllowedObjectsAreInFactory_LoadArray<OrgHeader>(true, delegate { return CreateRandomOrg(); }, OrgHeaderSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadArray<StorageDocs>(false, delegate { return CreateRandomStorageDoc(); }, StorageDocsSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadArray<StorageDocsUnallocated>(false, delegate { return CreateRandomStorageDocUnallocated(); }, StorageDocsSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadArray<StorageFile>(false, delegate { return CreateRandomStorageFile(); }, StorageDocsSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadArray<JobRequiredDocument>(true, delegate { return CreateRandomJobRequiredDocument(); }, JobRequiredDocumentSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadArray<RefDocType>(false, delegate { return CreateRandomRefDocType(); }, RefDocTypeSchema.PK);
		}

		protected void AssertOnlyAllowedObjectsAreInFactory_LoadArray<T>(bool shouldLoadInCommonFactory, CreateRandomObject create, SchemaPKColumn column) where T : BusinessObject
		{
			List<ZGuid> objs = new List<ZGuid>();
			objs.Add(create.Invoke().Key);
			objs.Add(create.Invoke().Key);
			objs.Add(create.Invoke().Key);
			objs.Add(create.Invoke().Key);
			objs.Add(create.Invoke().Key);
			SaveFactories();

			ZQuery query = new ZQuery(column, objs.ToArray());
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			BusinessObject[] assertObjs = docFactory.Load(typeof(T), query);
			AssertEquals(5, assertObjs.Length);
			foreach (BusinessObject obj in assertObjs)
			{
				AssertEquals(shouldLoadInCommonFactory, factory == obj.Factory);
				AssertEquals(!shouldLoadInCommonFactory, docFactory == obj.Factory);
			}

			factory = new BusinessObjectFactory();
			docFactory = GetFactory(factory);
			assertObjs = docFactory.Load<T>(query);
			AssertEquals(5, assertObjs.Length);
			foreach (BusinessObject obj in assertObjs)
			{
				AssertEquals(shouldLoadInCommonFactory, factory == obj.Factory);
				AssertEquals(!shouldLoadInCommonFactory, docFactory == obj.Factory);
			}
		}

		public virtual void TestOnlyAllowedObjectsAreInFactory_LoadOne()
		{
			AssertOnlyAllowedObjectsAreInFactory_LoadOne<OrgHeader>(true, delegate { return CreateRandomOrg(); }, OrgHeaderSchema.PK, "OH");
			AssertOnlyAllowedObjectsAreInFactory_LoadOne<StorageDocs>(false, delegate { return CreateRandomStorageDoc(); }, StorageDocsSchema.PK, "SC");
			AssertOnlyAllowedObjectsAreInFactory_LoadOne<StorageDocsUnallocated>(false, delegate { return CreateRandomStorageDocUnallocated(); }, StorageDocsSchema.PK, "SC");
			AssertOnlyAllowedObjectsAreInFactory_LoadOne<StorageFile>(false, delegate { return CreateRandomStorageFile(); }, StorageDocsSchema.PK, "SC");
			AssertOnlyAllowedObjectsAreInFactory_LoadOne<JobRequiredDocument>(true, delegate { return CreateRandomJobRequiredDocument(); }, JobRequiredDocumentSchema.PK, "EQ");
			AssertOnlyAllowedObjectsAreInFactory_LoadOne<RefDocType>(false, delegate { return CreateRandomRefDocType(); }, RefDocTypeSchema.PK, "RT");
		}

		protected void AssertOnlyAllowedObjectsAreInFactory_LoadOne<T>(bool shouldLoadInCommonFactory, CreateRandomObject create, SchemaPKColumn column, string prefix) where T : BusinessObject
		{
			ZGuid pk = create.Invoke().Key;
			SaveFactories();
			ZQuery query = new ZQuery(column, pk);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			BusinessObject assertObject;
			if (prefix == "OH")
			{
				assertObject = docFactory.Load(prefix, pk);
				AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
				AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);
			}

			factory = new BusinessObjectFactory();
			docFactory = new DocumentFactoryProvider().GetFactory(factory);
			assertObject = docFactory.Load(typeof(T), pk);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);

			factory = new BusinessObjectFactory();
			docFactory = GetFactory(factory);
			assertObject = docFactory.Load<T>(pk);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);
		}

		public virtual void TestOnlyAllowedObjectsAreInFactory_LoadTopOne()
		{
			AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<OrgHeader>(true, delegate { return CreateRandomOrg(); }, OrgHeaderSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<StorageDocs>(false, delegate { return CreateRandomStorageDoc(); }, StorageDocsSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<StorageDocsUnallocated>(false, delegate { return CreateRandomStorageDocUnallocated(); }, StorageDocsSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<StorageFile>(false, delegate { return CreateRandomStorageFile(); }, StorageDocsSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<JobRequiredDocument>(true, delegate { return CreateRandomJobRequiredDocument(); }, JobRequiredDocumentSchema.PK);
			AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<RefDocType>(false, delegate { return CreateRandomRefDocType(); }, RefDocTypeSchema.PK);
		}

		protected void AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<T>(bool shouldLoadInCommonFactory, CreateRandomObject create, SchemaPKColumn column) where T : BusinessObject
		{
			List<ZGuid> objs = new List<ZGuid>();
			objs.Add(create.Invoke().Key);
			objs.Add(create.Invoke().Key);
			SaveFactories();

			ZQuery query = new ZQuery(column, objs.ToArray());

			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			BusinessObject assertObject = docFactory.LoadTop1(typeof(T), query);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);

			factory = new BusinessObjectFactory();
			docFactory = GetFactory(factory);
			assertObject = docFactory.LoadTop1<T>(query);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);
		}

		public void TestOnlyAllowedObjectsAreInFactory_LoadFromNaturalKey()
		{
			AssertOnlyAllowedObjectsAreInFactory_LoadFromNaturalKey<OrgHeader>(true, delegate { return CreateRandomOrg(); }, OrgHeaderSchema.OH_Code);
		}

		void AssertOnlyAllowedObjectsAreInFactory_LoadFromNaturalKey<T>(bool shouldLoadInCommonFactory, CreateRandomObject create, SchemaColumn column) where T : BusinessObject
		{
			ZString code = new ZString(create.Invoke().Value.ToString());
			SaveFactories();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			BusinessObject assertObject = docFactory.LoadFromNaturalKey<T>(column, code);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);

			factory = new BusinessObjectFactory();
			docFactory = GetFactory(factory);
			assertObject = docFactory.LoadFromNaturalKey(typeof(T), column, code);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);
		}

		public virtual void TestOnlyAllowedObjectsAreInFactory_LoadFromUniqueKey()
		{
			AssertOnlyAllowedObjectsAreInFactory_LoadFromUniqueKey<GlbStaff>(true, delegate { return CreateRandomUser(); }, GlbStaffSchema.GS_Code);
		}

		protected void AssertOnlyAllowedObjectsAreInFactory_LoadFromUniqueKey<T>(bool shouldLoadInCommonFactory, CreateRandomObject create, SchemaColumn column) where T : BusinessObject
		{
			IZType key = create.Invoke().Value;
			SaveFactories();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			BusinessObject assertObject = docFactory.LoadFromUniqueKey(typeof(T), column, key);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);

			factory = new BusinessObjectFactory();
			docFactory = GetFactory(factory);
			assertObject = docFactory.LoadFromUniqueKey<T>(column, key);
			AssertEquals(shouldLoadInCommonFactory, factory == assertObject.Factory);
			AssertEquals(!shouldLoadInCommonFactory, docFactory == assertObject.Factory);
		}

		public virtual void TestOnlyAllowedObjectsAreInFactory_New()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);

			AssertEquals(factory, docFactory.New(typeof(OrgHeader)).Factory);
			AssertEquals(factory, docFactory.New(typeof(JobRequiredDocument)).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageDocs)).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageDocsUnallocated)).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageFile)).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(RefDocType)).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StmALog)).Factory);
		}

		public virtual void TestOnlyAllowedObjectsAreInFactory_NewTemplate()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);

			AssertEquals(factory, docFactory.New<OrgHeader>().Factory);
			AssertEquals(factory, docFactory.New<JobRequiredDocument>().Factory);
			AssertEquals(docFactory, docFactory.New<StorageDocs>().Factory);
			AssertEquals(docFactory, docFactory.New<StorageDocsUnallocated>().Factory);
			AssertEquals(docFactory, docFactory.New<StorageFile>().Factory);
			AssertEquals(docFactory, docFactory.New<RefDocType>().Factory);
			AssertEquals(docFactory, docFactory.New<StmALog>().Factory);
		}

		public virtual void TestOnlyAllowedObjectsAreInFactory_NewWithTypeAndPK()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);

			AssertEquals(factory, docFactory.New(typeof(OrgHeader), Guid.NewGuid()).Factory);
			AssertEquals(factory, docFactory.New(typeof(JobRequiredDocument), Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageDocs), Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageDocsUnallocated), Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageFile), Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(RefDocType), Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.New(typeof(StmALog), Guid.NewGuid()).Factory);
		}

		public virtual void TestOnlyAllowedObjectsAreInFactory_NewWithPrimaryKey()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);

			AssertEquals(factory, docFactory.NewWithPrimaryKey<OrgHeader>(Guid.NewGuid()).Factory);
			AssertEquals(factory, docFactory.NewWithPrimaryKey<JobRequiredDocument>(Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.NewWithPrimaryKey<StorageDocs>(Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.NewWithPrimaryKey<StorageDocsUnallocated>(Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.NewWithPrimaryKey<StorageFile>(Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.NewWithPrimaryKey<RefDocType>(Guid.NewGuid()).Factory);
			AssertEquals(docFactory, docFactory.NewWithPrimaryKey<StmALog>(Guid.NewGuid()).Factory);
		}

		public virtual void TestOnlyAllowedObjectsAreInFactoryAddToCollection()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);

			AssertEquals(factory, new OrgHeaderCollection(docFactory).AddNew().Factory);
			AssertEquals(factory, new JobRequiredDocumentCollection(docFactory).AddNew().Factory);
			AssertEquals(docFactory, new StorageDocsCollection(docFactory).AddNew().Factory);
			AssertEquals(docFactory, new StorageDocsUnallocatedCollection(docFactory).AddNew().Factory);
			AssertEquals(docFactory, new StorageFileCollection(docFactory).AddNew().Factory);
			AssertEquals(docFactory, new RefDocTypeCollection(docFactory).AddNew().Factory);
			AssertEquals(docFactory, new StmALogCollection(docFactory).AddNew().Factory);
		}
	}

	sealed class NumberedBusinessObjectFactoryTest : TransactionedTestCase
	{
		public void TestChildFactoryChangesUpdateCacheValueOnParent()
		{
			DocumentFactory parentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			NumberedBusinessObjectFactory childFactory = parentFactory.GetFactory(1);
			StorageDocs bo = childFactory.New<StorageDocs>();
			int parentFactoryCacheVersion = parentFactory.CacheVersion;
			int childFactoryCacheVersion = childFactory.CacheVersion;

			bo.SC_Desc = "Something Else";
			AssertEquals("Precondition: childFactory.CacheVersion should have gone up", childFactoryCacheVersion + 2, childFactory.CacheVersion);
			AssertEquals("parentFactory.CacheVersion should have gone up by one", parentFactoryCacheVersion + 2, parentFactory.CacheVersion);
		}

		public void TestDBNumber()
		{
			AssertEquals("Db number matches one passed in", 1, TestingFactory.DBNumber);

			TestingFactory = new NumberedBusinessObjectFactory(2, MasterFactory);
			AssertEquals("Db number matches one passed in", 2, TestingFactory.DBNumber);
		}

		public void TestMasterFactory()
		{
			AssertEquals("Master factory is same as what was passed in", MasterFactory, TestingFactory.MasterFactory);
			AssertEquals("Correct main factory", 0, TestingFactory.MasterFactory.DBNumber);
		}

		public void TestNewWithParent()
		{
			StorageMainCollection mainsPrevious = new StorageMainCollection(MasterFactory);
			mainsPrevious.Load();

			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(TestingFactory);
			AssertNotNull("CreateParentForDocument called in New(), Document now has a parent", doc.ParentMain);

			StorageMainCollection mainsAfter = new StorageMainCollection(MasterFactory);
			mainsAfter.Load();
			AssertEquals("Only one more storage main record created", mainsPrevious.Count + 1, mainsAfter.Count);
		}

		[ExpectException(typeof(EDocsOffLineException))]
		public void TestSaveToMissingeDocsDatabase()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			parent.SM_DB = 2;
			StorageDocs document = parent.Documents.AddNew();
			string dbName = new DocManagerDBHelper().GetDatabaseName(1);
			MasterFactory.Save();
		}

		public void TestSave_NoSilentExceptionIfMasterFactorySaved()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			parent.SM_DB = 1;
			StorageDocs document = parent.Documents.AddNew();

			AssertEquals("No silent error should be raised initially for the test", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			MasterFactory.Save();
			AssertEquals("No silent error should be raised when saving the document with the master factory", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestSave_RaiseSilentExceptionIfChildFactoryNotSavedWithMasterFactory()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			parent.SM_DB = 1;
			StorageDocs document = parent.Documents.AddNew();

			AssertEquals("No silent error should be raised initially for the test", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			document.Factory.Save();
			AssertEquals("A silent error should be raised", true, ErrorReporter.LastMessageReported.IndexOf("being saved without the Master factory") != -1);

			ErrorReporter.Clear();
		}

		public void TestSavingFactoryWorksForLogsAndForFileContents()
		{
			var staff = MasterFactory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";
			MasterFactory.Save();

			var sampleParent = MasterFactory.New<StorageMain>();
			sampleParent.SM_DB = 1;

			using var syncContext = SynchronizationContextForTest.Enable();
			using var resourceRetriever = new EmbeddedResourceRetriever();
			var tifFileContents = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
			var document = (StorageDocs)sampleParent.AddFileOrDocument(tifFileContents, new AddFileOrDocumentDto
			{
				FileName = "small.tif",
				DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument,
			});

			var pdfFileContents = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			using var newFile = (StorageFile)sampleParent.AddFileOrDocument(pdfFileContents, new AddFileOrDocumentDto
			{
				FileName = "Sample.PDF",
				DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument,
			});

			AssertEquals("Should have two eDocs", 2, sampleParent.eDocs.Count);
			AssertEquals("Should have one document", 1, sampleParent.Documents.Count);
			AssertEquals("Should have one file", 1, sampleParent.Files.Count);

			using (newFile.OpenForEdit())
			{
				System.Threading.Thread.Sleep(1000);

				File.AppendAllText(newFile.TempFileName, "Testing");
				Assert("Expected watcher_changed event to be raised", syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(3)));

				using (Env.SetTemporaryUserContext(staff.GS_LoginName, Guid.Empty, Guid.Empty))
				{
					MasterFactory.Save();
				}

				AssertEquals("AddingUser should return the test user", staff.GS_Code, document.SC_AddingUser);
				AssertEquals("AddingUser should return the test user", staff.GS_Code, newFile.SC_AddingUser);
				AssertEquals("File should have the new file contents", pdfFileContents.Length + 7, newFile.SC_ImageData.Length);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			TestingFactory = new NumberedBusinessObjectFactory(1, MasterFactory);
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DocManagerDBHelperTestClass dbHelper = new DocManagerDBHelperTestClass();
			if (!dbHelper.DatabaseExists(1))
			{
				dbHelper.CreateDatabase(1);
			}
		}

		DocumentFactory MasterFactory;
		NumberedBusinessObjectFactory TestingFactory;
	}

	public class NumberedBusinessObjectFactoryNonTransactionalTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestChangeDbNumberToAWriteableOne()
		{
			DocManagerDBHelper helper = new DocManagerDBHelper();
			string db1Name = helper.GetDatabaseName(1);
			string db2Name = helper.GetDatabaseName(2);

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(auxConnection, db1Name);
				AdoTestUtils.DropDbIfExists(auxConnection, db2Name);

				try
				{
					new DocManagerDBHelperTestClass().CreateDatabase(2);

					var numberedFactory = new NumberedBusinessObjectFactory(2, new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()));

					AssertEquals("DBNumber", 2, numberedFactory.DBNumber);
					numberedFactory.ChangeDbNumberToAWriteableOne(2);
					AssertEquals("DBNumber", 2, numberedFactory.DBNumber);

					try
					{
						numberedFactory.ChangeDbNumberToAWriteableOne(1);
						Fail("Should throw exception");
					}
					catch (InvalidOperationException) { }

					// Make DB2 read-only
					auxConnection.AlterDbWriteableStateForDocManager(db2Name, false);
					numberedFactory.ChangeDbNumberToAWriteableOne(1);
					AssertEquals("DBNumber", 1, numberedFactory.DBNumber);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(auxConnection, db2Name);
				}
			}
		}
	}
}
