using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Actions
{
	class ArchiveImageGenerationActionTestWithFactory : TestCaseWithFactory
	{
		public void TestErrorReportedForTooLargeArchiveReferenceKey()
		{
			_ = new ArchiveReferenceKey(new ReferenceKeyType("aaa", (NoResString)"bbb"), new string('a', StorageReferenceSchema.SR_Reference.MaxLength));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var archiveReferenceKey = new ArchiveReferenceKey(new ReferenceKeyType("aaa", (NoResString)"bbb"), new string('a', StorageReferenceSchema.SR_Reference.MaxLength + 1));
			AssertEquals($"ArchiveReferenceKey value is too long. Max length is 128, keyType = aaa/bbb, value = {archiveReferenceKey.Value}", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestBuildSql()
		{
			var sql = ArchiveImageGenerationAction.BuildSql();
			AssertEquals(
@"MERGE INTO StorageReference USING @referenceSetTVP
ON SR_SM = StorageMainPk AND SR_TYPE = ReferenceType AND SR_Reference = Reference
WHEN NOT MATCHED THEN
	INSERT (SR_PK, SR_SM, SR_Sequence, SR_TYPE, SR_Reference) VALUES (NEWID(), StorageMainPk, ReferenceSequence, ReferenceType, Reference);
", sql);
		}

		public void TestAddSqlParameters()
		{
			var refSet = new HashSet<ArchiveImageGenerationAction.StorageReferenceValues>();
			var pk1 = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770");
			var pk2 = new Guid("0EFE6607-5E7A-4F5A-A7ED-D44D7FECD08B");
			_ = refSet.Add(new ArchiveImageGenerationAction.StorageReferenceValues(pk1, new ArchiveReferenceKey(new ReferenceKeyType("C'1", (NoResString)"Code1"), "1'000"), 1));
			_ = refSet.Add(new ArchiveImageGenerationAction.StorageReferenceValues(pk1, new ArchiveReferenceKey(new ReferenceKeyType("C'1", (NoResString)"Code1"), "1'001"), 2));
			_ = refSet.Add(new ArchiveImageGenerationAction.StorageReferenceValues(pk2, new ArchiveReferenceKey(new ReferenceKeyType("C'2", (NoResString)"Code2"), "1'002"), 3));
			_ = refSet.Add(new ArchiveImageGenerationAction.StorageReferenceValues(pk2, new ArchiveReferenceKey(new ReferenceKeyType("C'2", (NoResString)"Code3"), "1'003"), 4));
			var sql = ArchiveImageGenerationAction.BuildSql();

			using var command = Db.Connection.Command(sql);
			ArchiveImageGenerationAction.AddSqlParameters(refSet, command);
			AssertEquals(1, command.ParameterCount);

			var param = command.GetParameter("@referenceSetTVP");

			Assert(param.Value.GetType().Equals(typeof(DataTable)));
			var dataTable = (DataTable)param.Value;

			AssertEquals(4, dataTable.Rows.Count);
			AssertEquals(4, dataTable.Columns.Count);

			var row0 = dataTable.Rows[0];
			AssertEquals(pk1, (Guid)row0["StorageMainPk"]);
			AssertEquals(1, (int)row0["ReferenceSequence"]);
			AssertEquals("C'1", (string)row0["ReferenceType"]);
			AssertEquals("1'000", (string)row0["Reference"]);

			var row1 = dataTable.Rows[1];
			AssertEquals(pk1, (Guid)row1["StorageMainPk"]);
			AssertEquals(2, (int)row1["ReferenceSequence"]);
			AssertEquals("C'1", (string)row1["ReferenceType"]);
			AssertEquals("1'001", (string)row1["Reference"]);

			var row2 = dataTable.Rows[2];
			AssertEquals(pk2, (Guid)row2["StorageMainPk"]);
			AssertEquals(3, (int)row2["ReferenceSequence"]);
			AssertEquals("C'2", (string)row2["ReferenceType"]);
			AssertEquals("1'002", (string)row2["Reference"]);

			var row3 = dataTable.Rows[3];
			AssertEquals(pk2, (Guid)row3["StorageMainPk"]);
			AssertEquals(4, (int)row3["ReferenceSequence"]);
			AssertEquals("C'2", (string)row3["ReferenceType"]);
			AssertEquals("1'003", (string)row3["Reference"]);
		}

		public void TestAddSQLParameters_WithReferenceLength_GreaterThan50()
		{
			var factory = new BusinessObjectFactory();
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageMain = documentFactory.NewWithValidTestData<StorageMain>();
			documentFactory.Save();

			var reference = "PFRCEUS2N77N50_63777509443737_176c9c04-aaa6-4900-a8bd-93a01a866b";
			var archiveReferenceKey = new ArchiveReferenceKey(new ReferenceKeyType("C'1", (NoResString)"Code1"), reference);
			var refSet = new HashSet<ArchiveImageGenerationAction.StorageReferenceValues>()
			{
				new(storageMain.PK, archiveReferenceKey, 1)
			};
			var sql = ArchiveImageGenerationAction.BuildSql();

			using var command = Db.Connection.Command(sql);
			ArchiveImageGenerationAction.AddSqlParameters(refSet, command);

			AssertNoExceptionThrown($"Expected Reference Value: '{reference}', with Length: {reference.Length}", () => command.ExecuteNonQuery());
		}

		public void TestCanHandleMoreThan525StorageReferences()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			var docSupport = dummyBizO as IDocManagerSupport;
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			masterFactory.NameForDebugging = "Document Factory";
			masterFactory.RefreshEnabled = false;
			var storageMain = masterFactory.RetrieveExistingOrCreateStorageMainForPK(dummyBizO.PK, dummyBizO, "DUM");
			masterFactory.Save();

			var refSet = new HashSet<ArchiveImageGenerationAction.StorageReferenceValues>();
			var pk1 = storageMain.PK;

			for (var i = 0; i < 526; i++)
			{
				_ = refSet.Add(new ArchiveImageGenerationAction.StorageReferenceValues(pk1, new ArchiveReferenceKey(new ReferenceKeyType($"{i}", (NoResString)$"Code{i}"), $"{i}'000"), i));
			}

			var sql = ArchiveImageGenerationAction.BuildSql();
			using (var command = Db.Connection.Command(sql))
			{
				ArchiveImageGenerationAction.AddSqlParameters(refSet, command);
				_ = command.ExecuteNonQuery();
			}

			AssertEquals("There should be 526 StorageReference records created for the StorageMain", 526, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.StorageReference WHERE SR_SM = '{pk1}'"));
		}

		public void TestLoggingErrorWhenGeneralExceptionThrown()
		{
			var action = new ArchiveImageGenerationAction();

			var docCommand = DocumentCommand.New(Factory);
			var printSet = new DocumentPrintSet(docCommand);
			_ = action.GenerateDocumentsToEDocsCore(null, printSet, docCommand, null).ToList();

			AssertContains("NRE was picked up by error reporter", "The following exception was thrown when generating eDocs during archiving:", ErrorReporter.LastMessageReported);
			AssertEquals("Thrown exception is of the right type", typeof(ArgumentNullException), ErrorReporter.LastExceptionReported.GetType());

			ErrorReporter.Clear();
		}
	}

	// A.K: I had to move these test out of test class inherited from TestCaseWithFactory, which in turn is inherited from TransactionedTestCase
	// reason being - if the test runs in transaction, threads other than main can't read rows not yet committed to DummyBizO table and therefore reloading BizOs in different factories doesn't work
	// this blows up ArchiveImageGenerationAction.GenerateDocumentsToEDocs(...) which run in Parellel.ForEach

	[UseSnapshotProtection]
	class ArchiveImageGenerationActionTestWithoutFactory : TestCase
	{
		const string DummyStageName = "Dummy Stage Name";

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocSpecifiedByIncludeDocInArchiveWillLogGeneratingWhenTheFileExists()
		{
			var factory = new BusinessObjectFactory();
			var archivePack = factory.New<StmMenuItem>();
			archivePack.SU_MenuName = "Arrival Notice For Brokerage";
			archivePack.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack.SU_ContactType = ContactType.ImportBroker.Code;
			archivePack.SU_MenuPath = "Arrival/Arrival Notice";
			archivePack.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			archivePack.SU_IncludeDocInArchive = "YES";

			var blankTemplate = factory.New<StmTemplate>();
			blankTemplate.SO_Name = "B";
			blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate.SO_DataContext = "UnitTest";

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = blankTemplate.PK;
			pivot.SI_SU = archivePack.PK;
			pivot.SI_DocumentTitle = "Test Doc";
			pivot.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			var archivePack1 = factory.New<StmMenuItem>();
			archivePack1.SU_MenuName = "Arrival Notice";
			archivePack1.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack1.SU_ContactType = ContactType.Consignee.Code;
			archivePack1.SU_MenuPath = "Arrival/Arrival Notice";
			archivePack1.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			archivePack1.SU_IncludeDocInArchive = "YES";

			var blankTemplate1 = factory.New<StmTemplate>();
			blankTemplate1.SO_Name = "B1";
			blankTemplate1.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate1.SO_DataContext = "UnitTest";

			var pivot1 = factory.New<StmMenuTemplatePivot>();
			pivot1.SI_SO = blankTemplate1.PK;
			pivot1.SI_SU = archivePack1.PK;
			pivot1.SI_DocumentTitle = "Test Doc";
			pivot1.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			var archivePack2 = factory.New<StmMenuItem>();
			archivePack2.SU_MenuName = "Arrival Notice";
			archivePack2.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack2.SU_ContactType = ContactType.NoContactType.Code;
			archivePack2.SU_MenuPath = "Arrival";
			archivePack2.SU_DocumentDirection = nameof(DocumentDirection.ANY);
			archivePack2.SU_IncludeDocInArchive = "YES";

			var blankTemplate2 = factory.New<StmTemplate>();
			blankTemplate2.SO_Name = "B2";
			blankTemplate2.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate2.SO_DataContext = "UnitTest";

			var pivot2 = factory.New<StmMenuTemplatePivot>();
			pivot2.SI_SO = blankTemplate2.PK;
			pivot2.SI_SU = archivePack2.PK;
			pivot2.SI_DocumentTitle = "Test Doc";
			pivot2.SI_RT_DocType = new Guid("42533766-6bdb-4b92-8525-a61cda021436"); //MSC

			var archivePack3 = factory.New<StmMenuItem>();
			archivePack3.SU_MenuName = "Arrival Notice (Automatic)";
			archivePack3.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack3.SU_ContactType = ContactType.Consignee.Code;
			archivePack3.SU_MenuPath = "Arrival/Arrival Notice";
			archivePack3.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			archivePack3.SU_IncludeDocInArchive = "YES";

			var blankTemplate3 = factory.New<StmTemplate>();
			blankTemplate3.SO_Name = "B3";
			blankTemplate3.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate3.SO_DataContext = "UnitTest";

			var pivot3 = factory.New<StmMenuTemplatePivot>();
			pivot3.SI_SO = blankTemplate3.PK;
			pivot3.SI_SU = archivePack3.PK;
			pivot3.SI_DocumentTitle = "Test Doc";
			pivot3.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			var staff1 = factory.New<GlbStaff>();
			staff1.GS_Code = "XX1";

			var company = factory.New<GlbCompany>();
			company.GC_Code = "CM1";

			var menu = factory.NewWithValidTestData<DocumentCommand>();
			menu.SU_MenuName = "Arrival Notice With Customs Deadline";
			menu.SU_BusinessContext = nameof(BusinessContext.Test);
			menu.SU_ContactType = ContactType.Consignee.Code;
			menu.SU_MenuPath = "Arrival/Arrival Notice";
			menu.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			menu.SU_IncludeDocInArchive = "YES";
			menu.SU_GS_NKStaffCode = staff1.GS_Code;
			menu.SU_FilterList = "";

			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(factory,
@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");
			template.SO_Name = "System Document Elements";
			template.SO_DataContext = "GenericFreightJob";

			var printCopyType = nameof(PrintCopyType.PRN);

			var pivot4 = factory.New<StmMenuTemplatePivotBase>();
			pivot4.SI_SO = template.PK;
			pivot4.SI_SU = menu.PK;
			pivot4.SI_DocumentTitle = "TestDoc";
			pivot4.SI_PrintCopyType = printCopyType;
			pivot4.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			var config = pivot4.DocConfigs.AddNew();
			config.S3_GC = company.PK;

			factory.Save();

			using (Env.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbCompany.CurrentCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				try
				{
					SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

					var cache = new ArchiveSystemCache();
					var dummyFactory = new BusinessObjectFactory();
					dummyFactory.RefreshEnabled = false;

					var dummy1 = dummyFactory.New<DummyWithDocumentSupport>();
					dummy1.Z0_Code = "1'111";
					dummyFactory.Save();

					var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
					docFactory.RefreshEnabled = false;

					var dummy1Main = docFactory.New<StorageMain>();
					dummy1Main.SM_DB = DbNumber;
					dummy1Main.SM_ParentFK = dummy1.PK;

					var docFactoryForDB1 = docFactory.GetFactory(DbNumber);
					var doc1 = docFactoryForDB1.New<StorageDocs>();
					doc1.SC_SM = dummy1Main.PK;
					doc1.SC_FileName = "testfile.pdf";
					doc1.SC_DocType = "AGI";
					doc1.SC_ImageData = TestFileHelper.SamplePDF;

					docFactory.Save();
					dummyFactory.Save();
					var logger = new TestArchiveLogger();
					var descriptor = new DummyArchiveSystem();
					var archiveItem = new ArchiveItem(dummy1.PKSchemaColumn, dummy1.PK.ToGuid());
					var testSet = new TestArchiveSet(descriptor, DummyStageName, Guid.Empty, archiveItem);
					var providerDictionary = new ArchiveableBusinessObjectProviderCache();
					providerDictionary.RegisterProvider(new DummyArchiveableBusinessObjectProvider());
					var action = new ArchiveImageGenerationAction();
					action.Setup(logger, testSet, providerDictionary, cache);

					action.Execute();

					var expectedElements = new List<string> { $"Information|DMY|Generating missing Arrival Notice (Automatic)",
					"Information|DMY|Generating missing Arrival Notice For Brokerage",
					"Information|DMY|Generating missing Arrival Notice",
					"Information|DMY|Generating missing Arrival Notice",
					};
					AssertContainsExactElementsInAnyOrder(expectedElements, logger.ListOfMessages);
				}
				finally
				{
					SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				}
			}
		}

		public void TestReproduceEmptyDocTypeError()
		{
			var factory = new BusinessObjectFactory();

			var userDefinedDocType = factory.New<RefDocType>();
			userDefinedDocType.RT_DocType = "SHP";
			userDefinedDocType.RT_Desc = "Shipping Documents";
			userDefinedDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			userDefinedDocType.RT_SE_NKDocumentReceivedEvent = AutoEvents.DocumentImportedCode;
			userDefinedDocType.RT_IsActive = true;
			userDefinedDocType.RT_IsPublished = true;
			userDefinedDocType.RT_OverrideVersions = true;

			var bookingConfirmation = factory.Load<StmMenuItem>(new ZGuid("4A056A5B-240F-43AF-AA2C-44E0DF69556B"));

			var menuEDocs = factory.New<StmMenuEDocs>();
			menuEDocs.SX_SU = bookingConfirmation.PK;
			menuEDocs.SX_RT_DocType = userDefinedDocType.PK;

			var jobShipment = factory.New<ForwardingShipment>();

			var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = jobShipment.PK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			docFactory.RefreshEnabled = false;

			var dummy1Main = docFactory.New<StorageMain>();
			dummy1Main.SM_DB = DbNumber;
			dummy1Main.SM_ParentFK = jobShipment.PK;
			dummy1Main.SM_Type = userDefinedDocType.RT_DocType;

			var docFactoryForDB1 = docFactory.GetFactory(DbNumber);
			var doc1 = docFactoryForDB1.New<StorageDocs>();
			doc1.SC_SM = dummy1Main.PK;
			doc1.SC_FileName = "testfile";
			doc1.SC_DocType = userDefinedDocType.RT_DocType;
			doc1.SC_ImageData = new byte[] { 1, 2, 3 };
			doc1.SC_IsPublished = true;

			docFactory.Save();
			factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(factory, ArchiveManagerConstants.Codes.OPS);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.OPS, configuration, logger, schedule);

			CombineAssertions(() =>
			{
				AssertEquals("JobHeader and JobShipment should have been archived", 2, factory.GetDatabaseCount(typeof(StorageReference), new ZQuery(StorageReferenceSchema.SR_Reference, jobHeader.JH_JobNum)));
				AssertNull("No exception should be thrown.", ErrorReporter.LastExceptionReported);
			});
		}

		public void TestGenerateDocumentsToEDocsCore_WhenStmMenuTemplatePivotHasEmptyDocType()
		{
			var factory = new BusinessObjectFactory();

			var menuItem = factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Arrival Notice - Test";
			menuItem.SU_BusinessContext = "Shipment";
			menuItem.SU_IsPublished = true;
			menuItem.SU_MenuPath = "Test/Arrival/ArrivalNotice";
			menuItem.SU_MenuShortcut = "None";
			menuItem.SU_IncludeDocInArchive = "YES";
			menuItem.SU_ContactType = "CNE";
			menuItem.SU_AddressCategory = "OFF";
			menuItem.SU_DocumentDirection = "ARV";

			var template = factory.Load<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, "System Document Elements"))[0];

			var menuTemplatePivotWithoutDocType = factory.New<StmMenuTemplatePivot>();
			menuTemplatePivotWithoutDocType.SI_SU = menuItem.PK;
			menuTemplatePivotWithoutDocType.SI_SO = template.PK;
			menuTemplatePivotWithoutDocType.SI_DocumentTitle = "Arrival Notice - No DocType";

			var menuTemplatePivot = factory.New<StmMenuTemplatePivot>();
			menuTemplatePivot.SI_SU = menuItem.PK;
			menuTemplatePivot.SI_SO = template.PK;
			menuTemplatePivot.SI_DocumentTitle = "Arrival Notice - Has DocType";
			menuTemplatePivot.SI_RT_DocType = factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ARN"))[0].PK;

			var menuDocumentConfig = factory.New<StmMenuDocumentConfig>();
			menuDocumentConfig.S3_SI = menuTemplatePivotWithoutDocType.PK;
			menuDocumentConfig.S3_Description = "Default";

			var menuDocumentConfigItem = factory.New<StmMenuDocumentConfigItem>();
			menuDocumentConfigItem.S4_SectionType = "HFP";
			menuDocumentConfigItem.S4_SectionItemName = "Company Logo";
			menuDocumentConfigItem.S4_S3 = menuDocumentConfig.PK;
			menuDocumentConfigItem.S4_PrintOrder = 1;
			menuDocumentConfigItem.S4_IsSystemDefined = true;

			var jobShipment = factory.New<ForwardingShipment>();

			var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = jobShipment.PK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;

			factory.Save();

			Assert("Precondition: StmMenuTemplatePivot should have no DocType.", menuTemplatePivotWithoutDocType.SI_RT_DocType.IsEmpty);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(factory, ArchiveManagerConstants.Codes.OPS);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.OPS, configuration, logger, schedule);

			CombineAssertions(() =>
			{
				AssertNull("No exception should be thrown when there is an invalid DocType during archiving.", ErrorReporter.LastExceptionReported);
				Assert("There should be a log message about the invalid DocType.", logger.ListOfMessages.Exists(l => l.Contains("Warning|OPS|Archiving process cannot generate document for")));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTimeTakenIsIncludedInGeneratingMissingDocumentsLogByDefault()
		{
			AssertEquals("Time Taken should be ON by default", true, SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value);

			PrepareDocumentMenuToBeIncludedInArchive();

			var cache = new ArchiveSystemCache();
			var dummyFactory = new BusinessObjectFactory();
			dummyFactory.RefreshEnabled = false;

			var dummy1 = dummyFactory.New<DummyWithDocumentSupport>();
			dummy1.Z0_Code = "1'111";
			dummyFactory.Save();

			var logger = new TestArchiveLogger();
			var descriptor = new DummyArchiveSystem();
			var archiveItem = new ArchiveItem(dummy1.PKSchemaColumn, dummy1.PK.ToGuid());
			var testSet = new TestArchiveSet(descriptor, DummyStageName, Guid.Empty, archiveItem);
			var providerDictionary = new ArchiveableBusinessObjectProviderCache();
			providerDictionary.RegisterProvider(new DummyArchiveableBusinessObjectProvider());

			var action = new ArchiveImageGenerationAction();
			action.Setup(logger, testSet, providerDictionary, cache);
			action.Execute();

			AssertContains("Generating missing Archive Test Document Time taken:", logger.ListOfMessages[0]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocSpecifiedByArchiveDocumentInterface()
		{
			var factory = new BusinessObjectFactory();
			var archivePack = factory.New<StmMenuItem>();
			archivePack.SU_MenuName = "Archive Pack";
			archivePack.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack.SU_IsDocPack = true;
			archivePack.SU_ContactType = ContactType.NoContactType.Code;

			var blankTemplate = factory.New<StmTemplate>();
			blankTemplate.SO_Name = "B";
			blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate.SO_DataContext = "UnitTest";

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = blankTemplate.PK;
			pivot.SI_SU = archivePack.PK;
			pivot.SI_DocumentTitle = "Test Doc";
			pivot.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			factory.Save();

			var cache = new ArchiveSystemCache();
			RunTestCore(cache);
		}

		void PrepareDocumentMenuToBeIncludedInArchive()
		{
			var factory = new BusinessObjectFactory();

			var archivePack = factory.New<StmMenuItem>();
			archivePack.SU_MenuName = "Archive Test Document";
			archivePack.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack.SU_IncludeDocInArchive = "YES";
			archivePack.SU_ContactType = ContactType.NoContactType.Code;

			var blankTemplate = factory.New<StmTemplate>();
			blankTemplate.SO_Name = "B";
			blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate.SO_DataContext = "UnitTest";

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = blankTemplate.PK;
			pivot.SI_SU = archivePack.PK;
			pivot.SI_DocumentTitle = "Test Doc";
			pivot.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			//insert a menu that wont be generated
			var archivePack2 = factory.New<StmMenuItem>();
			archivePack2.SU_MenuName = "Archive Test Document2";
			archivePack2.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack2.SU_IncludeDocInArchive = "NON";
			archivePack2.SU_ContactType = ContactType.NoContactType.Code;

			var blankTemplate2 = factory.New<StmTemplate>();
			blankTemplate2.SO_Name = "B2";
			blankTemplate2.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate2.SO_DataContext = "UnitTest";

			var pivot2 = factory.New<StmMenuTemplatePivot>();
			pivot2.SI_SO = blankTemplate2.PK;
			pivot2.SI_SU = archivePack2.PK;
			pivot2.SI_DocumentTitle = "Test Doc2";
			pivot2.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			factory.Save();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocSpecifiedByIncludeDocInArchive()
		{
			PrepareDocumentMenuToBeIncludedInArchive();

			var cache = new ArchiveSystemCache();
			RunTestCore(cache);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocSpecifiedByIncludeDocInArchiveAfterSimulationRun()
		{
			PrepareDocumentMenuToBeIncludedInArchive();

			var cache = new ArchiveSystemCache();
			RunTestCore(cache, sm => sm.SM_Archived = ZDateTime.BrettsBirthday);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocSpecifiedByLongDocumentName()
		{
			var factory = new BusinessObjectFactory();
			var archivePack = factory.New<StmMenuItem>();
			archivePack.SU_MenuName = "Archive Pack";
			archivePack.SU_BusinessContext = nameof(BusinessContext.Test);
			archivePack.SU_IsDocPack = true;
			archivePack.SU_ContactType = ContactType.NoContactType.Code;

			var blankTemplate = factory.New<StmTemplate>();
			blankTemplate.SO_Name = "B";
			blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate.SO_DataContext = "UnitTest";

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = blankTemplate.PK;
			pivot.SI_SU = archivePack.PK;
			pivot.SI_DocumentTitle = "EPR-AUCustoms EntryPrint (Landscape) - 2M50192316P,2M50192345E,2M50192347J,2M50192338H,2M50192350B,2M50192332P,2M50192331N,2M50192333A,2M50192336D,2M50192340P,2M50192353E,2M50192349M,2M50192344D,2M50192339J,2M50192341A";
			pivot.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

			factory.Save();

			var cache = new ArchiveSystemCache();
			RunTestCore(cache);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocWithFileFormatPDF()
		{
			var factory = new BusinessObjectFactory();
			using (SystemDataRegistry.Instance.OnlineArchiveDocumentFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.PDF))
			{
				var archivePack = factory.New<StmMenuItem>();
				archivePack.SU_MenuName = "Archive Pack";
				archivePack.SU_BusinessContext = nameof(BusinessContext.Test);
				archivePack.SU_IsDocPack = true;
				archivePack.SU_ContactType = ContactType.NoContactType.Code;

				var blankTemplate = factory.New<StmTemplate>();
				blankTemplate.SO_Name = "B";
				blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
				blankTemplate.SO_DataContext = "UnitTest";

				var pivot = factory.New<StmMenuTemplatePivot>();
				pivot.SI_SO = blankTemplate.PK;
				pivot.SI_SU = archivePack.PK;
				pivot.SI_DocumentTitle = "Test Doc";
				pivot.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

				factory.Save();

				var cache = new ArchiveSystemCache();
				RunTestCore(cache);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocWithFileFormatPDFA()
		{
			var factory = new BusinessObjectFactory();
			using (SystemDataRegistry.Instance.OnlineArchiveDocumentFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.PDFA))
			{
				var archivePack = factory.New<StmMenuItem>();
				archivePack.SU_MenuName = "Archive Pack";
				archivePack.SU_BusinessContext = nameof(BusinessContext.Test);
				archivePack.SU_IsDocPack = true;
				archivePack.SU_ContactType = ContactType.NoContactType.Code;

				var blankTemplate = factory.New<StmTemplate>();
				blankTemplate.SO_Name = "B";
				blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
				blankTemplate.SO_DataContext = "UnitTest";

				var pivot = factory.New<StmMenuTemplatePivot>();
				pivot.SI_SO = blankTemplate.PK;
				pivot.SI_SU = archivePack.PK;
				pivot.SI_DocumentTitle = "Test Doc";
				pivot.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

				factory.Save();

				var cache = new ArchiveSystemCache();
				RunTestCore(cache);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDualJobsGenerateALLeDocsWhenArchived()
		{
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var factory = new BusinessObjectFactory();

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();

			forwardingTestDataCreator.CreateConsolData(1, 0, out var consol1PK, isCancelled: false, out var shipmentPKs, out var outPutindex, isCFSLoadList: true);

			var oldDate = new ZDateTime(2007, 1, 10);
			var jobClosedDate = new ZDateTime(2007, 1, 20);

			var jobHeaderConsol1 = factory.NewJobForTesting<JobHeader>();
			jobHeaderConsol1.JH_GB = Env.CurrentBranch.PK;
			jobHeaderConsol1.JH_GE = Env.CurrentDepartment.PK;
			jobHeaderConsol1.JH_JobNum = "3333";
			jobHeaderConsol1.JH_SystemLastEditTimeUtc = oldDate;
			jobHeaderConsol1.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobHeaderConsol1.JH_ParentID = consol1PK;
			jobHeaderConsol1.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeaderConsol1.JH_A_JCL = jobClosedDate;
			factory.Save();

			var docType1 = factory.NewWithValidTestData<RefDocType>();
			var docType2 = factory.NewWithValidTestData<RefDocType>();

			// someone bumped RF_DocType's lenght from 3 to 4 and test now fails
			// I expect this change might cause some amnesties
			// using old school 3 char code for thos test for the time being

			docType1.RT_DocType = docType1.RT_DocType.Substring(0, docType1.RT_DocType.Length - 1);
			docType2.RT_DocType = docType2.RT_DocType.Substring(0, docType2.RT_DocType.Length - 1);

			var forwardingConsolPack = factory.New<StmMenuItem>();
			forwardingConsolPack.SU_MenuName = "Archive Pack For Forwarding Consol";
			forwardingConsolPack.SU_BusinessContext = nameof(BusinessContext.Consol);
			forwardingConsolPack.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			forwardingConsolPack.SU_IsDocPack = true;
			forwardingConsolPack.SU_ContactType = ContactType.ImportBroker.Code;
			forwardingConsolPack.SU_IncludeDocInArchive = "YES";
			forwardingConsolPack.SU_MenuPath = "Arrival/Arrival Notice";

			var forwardingShipmentPack = factory.New<StmMenuItem>();
			forwardingShipmentPack.SU_MenuName = "Archive Pack For Forwarding Shipment";
			forwardingShipmentPack.SU_BusinessContext = nameof(BusinessContext.Shipment);
			forwardingShipmentPack.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			forwardingShipmentPack.SU_IsDocPack = true;
			forwardingShipmentPack.SU_ContactType = ContactType.ImportBroker.Code;
			forwardingShipmentPack.SU_IncludeDocInArchive = "YES";
			forwardingShipmentPack.SU_MenuPath = "Arrival/Arrival Notice";

			var cfsLoadListConsolPack = factory.New<StmMenuItem>();
			cfsLoadListConsolPack.SU_MenuName = "Archive Pack For CFS Load List Consol";
			cfsLoadListConsolPack.SU_BusinessContext = nameof(BusinessContext.CFSLoadList);
			cfsLoadListConsolPack.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			cfsLoadListConsolPack.SU_IsDocPack = true;
			cfsLoadListConsolPack.SU_ContactType = ContactType.NoContactType.Code;
			cfsLoadListConsolPack.SU_IncludeDocInArchive = "YES";
			cfsLoadListConsolPack.SU_MenuPath = "Arrival/Arrival Notice";

			var cfsShipmentPack = factory.New<StmMenuItem>();
			cfsShipmentPack.SU_MenuName = "Archive Pack For CFS Shipment";
			cfsShipmentPack.SU_BusinessContext = nameof(BusinessContext.CFSShipmentReceival);
			cfsShipmentPack.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			cfsShipmentPack.SU_IsDocPack = true;
			cfsShipmentPack.SU_ContactType = ContactType.NoContactType.Code;
			cfsShipmentPack.SU_IncludeDocInArchive = "YES";
			cfsShipmentPack.SU_MenuPath = "Arrival/Arrival Notice";

			var blankTemplate = factory.New<StmTemplate>();
			blankTemplate.SO_Name = "B";
			blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate.SO_DataContext = "GenericFreightJob";

			var forwardingConsolPivot = factory.New<StmMenuTemplatePivot>();
			forwardingConsolPivot.SI_SO = blankTemplate.PK;
			forwardingConsolPivot.SI_SU = forwardingConsolPack.PK;
			forwardingConsolPivot.SI_DocumentTitle = "Test Doc Forwarding Consol";
			forwardingConsolPivot.SI_RT_DocType = docType1.PK;

			var forwardingShipmentlPivot = factory.New<StmMenuTemplatePivot>();
			forwardingShipmentlPivot.SI_SO = blankTemplate.PK;
			forwardingShipmentlPivot.SI_SU = forwardingShipmentPack.PK;
			forwardingShipmentlPivot.SI_DocumentTitle = "Test Doc Forwarding Shipment";
			forwardingShipmentlPivot.SI_RT_DocType = docType1.PK;

			var cfsLoadListConsolPivot = factory.New<StmMenuTemplatePivot>();
			cfsLoadListConsolPivot.SI_SO = blankTemplate.PK;
			cfsLoadListConsolPivot.SI_SU = cfsLoadListConsolPack.PK;
			cfsLoadListConsolPivot.SI_DocumentTitle = "Test Doc CFS Load List Consol";
			cfsLoadListConsolPivot.SI_RT_DocType = docType2.PK;

			var cfsShipmentPivot = factory.New<StmMenuTemplatePivot>();
			cfsShipmentPivot.SI_SO = blankTemplate.PK;
			cfsShipmentPivot.SI_SU = cfsShipmentPack.PK;
			cfsShipmentPivot.SI_DocumentTitle = "Test Doc CFS Shipment";
			cfsShipmentPivot.SI_RT_DocType = docType2.PK;

			factory.Save();

			var manager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 15), 10, ZDateTime.UtcNow, true, shouldIncludeDeclarations: false);

			manager.Run(ArchiveManagerConstants.Codes.OPS, config, logger, schedule, new CancellationToken());

			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var consolStorageMain = masterFactory.GetStorageMainForPK(consol1PK);
			var shipmentStorageMain = masterFactory.GetStorageMainForPK(shipmentPKs[0]);

			Assert("Archived date on Consol StorageMain", consolStorageMain.SM_Archived != ZDateTime.Empty);
			Assert("Archived date on Shipment StorageMain", shipmentStorageMain.SM_Archived != ZDateTime.Empty);

			AssertEquals("Document for Forwarding consol was generated", 1, consolStorageMain.eDocs.Find(eDoc => eDoc.SC_Desc.Equals("Archive Pack For Forwarding Consol")).Count());
			AssertEquals("Document for Forwarding Shipment was generated", 1, shipmentStorageMain.eDocs.Find(eDoc => eDoc.SC_Desc.Equals("Archive Pack For Forwarding Shipment")).Count());
			AssertEquals("Document for CFS Load List was generated", 1, consolStorageMain.eDocs.Find(eDoc => eDoc.SC_Desc.Equals("Archive Pack For CFS Load List Consol")).Count());
			AssertEquals("Document for CFS Shipment was generated", 1, shipmentStorageMain.eDocs.Find(eDoc => eDoc.SC_Desc.Equals("Archive Pack For CFS Shipment")).Count());
		}

		#region Handles Leak Tests

		[DeveloperOnlyTest]
		public void TestHandlesDontLeak()
		{
			var initialNumberOfStorageMains = GetNumberOfConsolArchivedStorageMain();

			SetupMenuItems();

			//warm up run
			RunArchiveManagerProcess();

			GC.WaitForPendingFinalizers();
			var handlesBeforeRun = GetCurrentHandleUsage();

			for (var i = 0; i < 4; i++)
			{
				RunArchiveManagerProcess();
			}

			GC.WaitForPendingFinalizers();
			var handlesAfterRun = GetCurrentHandleUsage();

			AssertEquals("StorageMains for 50 newly added Consols were created by archiving process and are marked as archived", initialNumberOfStorageMains + 50, GetNumberOfConsolArchivedStorageMain());
			AssertLessThan("Handles leaked after Single Threaded Run", handlesAfterRun - handlesBeforeRun, leakingHandlesThreshold);
		}

		readonly int leakingHandlesThreshold = 100;

		void SetupMenuItems()
		{
			var factory = new BusinessObjectFactory();
			var docType1 = factory.NewWithValidTestData<RefDocType>();

			var forwardingConsolPack = factory.New<StmMenuItem>();
			forwardingConsolPack.SU_MenuName = "Archive Pack For Forwarding Consol";
			forwardingConsolPack.SU_BusinessContext = nameof(BusinessContext.Consol);
			forwardingConsolPack.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			forwardingConsolPack.SU_IsDocPack = true;
			forwardingConsolPack.SU_ContactType = ContactType.ImportBroker.Code;
			forwardingConsolPack.SU_IncludeDocInArchive = "YES";
			forwardingConsolPack.SU_MenuPath = "Arrival/Arrival Notice";

			var forwardingShipmentPack = factory.New<StmMenuItem>();
			forwardingShipmentPack.SU_MenuName = "Archive Pack For Forwarding Shipment";
			forwardingShipmentPack.SU_BusinessContext = nameof(BusinessContext.Shipment);
			forwardingShipmentPack.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			forwardingShipmentPack.SU_IsDocPack = true;
			forwardingShipmentPack.SU_ContactType = ContactType.ImportBroker.Code;
			forwardingShipmentPack.SU_IncludeDocInArchive = "YES";
			forwardingShipmentPack.SU_MenuPath = "Arrival/Arrival Notice";

			var blankTemplate = factory.New<StmTemplate>();
			blankTemplate.SO_Name = "B";
			blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
			blankTemplate.SO_DataContext = "GenericFreightJob";

			var forwardingConsolPivot = factory.New<StmMenuTemplatePivot>();
			forwardingConsolPivot.SI_SO = blankTemplate.PK;
			forwardingConsolPivot.SI_SU = forwardingConsolPack.PK;
			forwardingConsolPivot.SI_DocumentTitle = "Test Doc Forwarding Consol";
			forwardingConsolPivot.SI_RT_DocType = docType1.PK;

			var forwardingShipmentlPivot = factory.New<StmMenuTemplatePivot>();
			forwardingShipmentlPivot.SI_SO = blankTemplate.PK;
			forwardingShipmentlPivot.SI_SU = forwardingShipmentPack.PK;
			forwardingShipmentlPivot.SI_DocumentTitle = "Test Doc Forwarding Shipment";
			forwardingShipmentlPivot.SI_RT_DocType = docType1.PK;

			factory.Save();
		}

		void RunArchiveManagerProcess()
		{
			for (var i = 0; i < 10; i++)
			{
				CreateConsol(i);
			}

			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var manager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 15), 10, ZDateTime.UtcNow, true, shouldIncludeDeclarations: false);

			manager.Run(ArchiveManagerConstants.Codes.OPS, config, logger, schedule, new CancellationToken());
		}

		void CreateConsol(int jobNumber)
		{
			var factory = new BusinessObjectFactory();

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();

			forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: false, out _, out _);

			var oldDate = new ZDateTime(2007, 1, 10);
			var jobClosedDate = new ZDateTime(2007, 1, 20);

			var jobHeaderConsol1 = factory.NewJobForTesting<JobHeader>();
			jobHeaderConsol1.JH_GB = Env.CurrentBranch.PK;
			jobHeaderConsol1.JH_GE = Env.CurrentDepartment.PK;
			jobHeaderConsol1.JH_JobNum = jobNumber.ToString();
			jobHeaderConsol1.JH_SystemLastEditTimeUtc = oldDate;
			jobHeaderConsol1.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobHeaderConsol1.JH_ParentID = consol1PK;
			jobHeaderConsol1.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeaderConsol1.JH_A_JCL = jobClosedDate;
			factory.Save();
		}

		static int GetCurrentHandleUsage()
		{
			using var currentProcess = Process.GetCurrentProcess();
			currentProcess.Refresh();
			return currentProcess.HandleCount;
		}

		int GetNumberOfConsolArchivedStorageMain()
			=> (int)Db.Connection.ExecuteScalar("select count(SM_PK) from dbo.StorageMain where SM_Archived is not null and SM_Type = 'CON'");

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDocWithFileFormatTIF()
		{
			var factory = new BusinessObjectFactory();
			using (SystemDataRegistry.Instance.OnlineArchiveDocumentFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.TIF))
			{
				var archivePack = factory.New<StmMenuItem>();
				archivePack.SU_MenuName = "Archive Pack";
				archivePack.SU_BusinessContext = nameof(BusinessContext.Test);
				archivePack.SU_IsDocPack = true;
				archivePack.SU_ContactType = ContactType.NoContactType.Code;

				var blankTemplate = factory.New<StmTemplate>();
				blankTemplate.SO_Name = "B";
				blankTemplate.SO_Template = System.IO.File.ReadAllBytes(UnitTestingConstants.TestReportExcelTemplateFilePath);
				blankTemplate.SO_DataContext = "UnitTest";

				var pivot = factory.New<StmMenuTemplatePivot>();
				pivot.SI_SO = blankTemplate.PK;
				pivot.SI_SU = archivePack.PK;
				pivot.SI_DocumentTitle = "Test Doc";
				pivot.SI_RT_DocType = new Guid("FFA1A458-1F15-4AAD-9C4A-F335A860C770"); //MSC

				factory.Save();

				var cache = new ArchiveSystemCache();
				RunTestCore(cache);
			}
		}

		void RunTestCore(IArchiveSystemCache systemCache, Action<StorageMain> extraSetupForStorageMain = null)
		{
			var dummyFactory = new BusinessObjectFactory();
			dummyFactory.RefreshEnabled = false;

			var dummy1 = dummyFactory.New<DummyWithDocumentSupport>();
			dummy1.Z0_Code = "1'111";
			dummyFactory.Save();

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			docFactory.RefreshEnabled = false;

			var dummy1Main = docFactory.New<StorageMain>();
			dummy1Main.SM_DB = DbNumber;
			dummy1Main.SM_ParentFK = dummy1.PK;

			extraSetupForStorageMain?.Invoke(dummy1Main);

			var docFactoryForDB = docFactory.GetFactory(DbNumber);
			var doc1 = docFactoryForDB.New<StorageDocs>();
			doc1.SC_SM = dummy1Main.PK;
			doc1.SC_FileName = "testfile.pdf";
			doc1.SC_DocType = "AGI";
			doc1.SC_ImageData = TestFileHelper.SamplePDF;

			docFactory.Save();

			dummyFactory.Save();

			ExecuteAction(dummy1, 1, systemCache);
			ExecuteAction(dummy1, 1, systemCache); // should not need to generate again, as it's already generated.
		}

		void ExecuteAction(DummyBusinessObject dummy1, int numberOfeDocsCreated, IArchiveSystemCache systemCache)
		{
			var logger = new TestArchiveLogger();
			var descriptor = new DummyArchiveSystem();
			var mainArchiveItem = new ArchiveItem(dummy1.PKSchemaColumn, dummy1.PK.ToGuid());
			var testSet = new TestArchiveSet(descriptor, "Dummy Stage Name", Guid.Empty, mainArchiveItem);

			var providerDictionary = new ArchiveableBusinessObjectProviderCache();
			providerDictionary.RegisterProvider(new DummyArchiveableBusinessObjectProvider());
			var action = new ArchiveImageGenerationAction();
			action.Setup(logger, testSet, providerDictionary, systemCache);

			action.Execute();

			var dummyReloadFactory = new BusinessObjectFactory();
			var dummy1Reloaded = dummyReloadFactory.Load<DummyWithDocumentSupport>(dummy1.PK);

			TestDocManagerInfo(dummy1Reloaded, numberOfeDocsCreated);

			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var dummy1MainLoaded = masterFactory.GetStorageMainForPK(dummy1Reloaded.PK);
			Assert("Archived date", dummy1MainLoaded.SM_Archived != ZDateTime.Empty);

			var query = new ZQuery(StorageReferenceSchema.SR_SM, dummy1MainLoaded.PK);
			query.OrderBy = AutoStorageReference.Schema.SR_Sequence;
			var storageReferences = masterFactory.Load<StorageReference>(query);
			AssertEquals("storageReferences count", 1, storageReferences.Length);
			AssertEquals("Type", "DUM", storageReferences[0].SR_TYPE);
			AssertEquals("Reference", "1'111", storageReferences[0].SR_Reference);
		}

		void TestDocManagerInfo(DummyWithDocumentSupport dummyReloaded, int numberOfeDocsCreated)
		{
			if (SystemDataRegistry.Instance.OnlineArchiveDocumentFormat.Value == Core.Constants.FileFormats.TIF)
			{
				AssertEquals("Total eDocs Count", numberOfeDocsCreated + 1, dummyReloaded.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Documents Count", numberOfeDocsCreated + 1, dummyReloaded.DocManagerInfo.Documents.Count);
				AssertEquals("Files Count", 0, dummyReloaded.DocManagerInfo.Files.Count);
			}
			else
			{
				AssertEquals("Total eDocs Count", numberOfeDocsCreated + 1, dummyReloaded.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Documents Count", 1, dummyReloaded.DocManagerInfo.Documents.Count);
				AssertEquals("Files Count", numberOfeDocsCreated, dummyReloaded.DocManagerInfo.Files.Count);
			}

			AssertEquals("Doc 1 DocType", "AGI", dummyReloaded.DocManagerInfo.Documents[0].DocType);
			Assert("Doc 1 Image exists", dummyReloaded.DocManagerInfo.Documents[0].ImageData.Length > 0);

			for (var index = 1; index < dummyReloaded.DocManagerInfo.Documents.Count; index++)
			{
				var doc = dummyReloaded.DocManagerInfo.Documents[index];
				var expectedDocType = "MSC";
				if (doc.FileName.Contains("testfile2"))
				{
					expectedDocType = "TST";
				}

				AssertEquals(string.Format("Doc {0} DocType", index), expectedDocType, doc.DocType);
				Assert(string.Format("Doc {0} Image exists", index), doc.ImageData.Length > 0);
			}

			for (var index = 0; index < dummyReloaded.DocManagerInfo.Files.Count; index++)
			{
				var doc = dummyReloaded.DocManagerInfo.Files[index];
				var expectedDocType = "MSC";
				if (doc.FileName.Contains("testfile2"))
				{
					expectedDocType = "TST";
				}

				AssertEquals(string.Format("Doc {0} DocType", index), expectedDocType, doc.DocType);
				Assert(string.Format("Doc {0} Image exists", index), doc.ImageData.Length > 0);
				AssertEquals("Doc {0} DataType", SystemDataRegistry.Instance.OnlineArchiveDocumentFormat.Value, doc.DataType);
			}
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			if (!dbHelper.DatabaseExists(DbNumber))
			{
				_ = dbHelper.CreateDatabase(DbNumber);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
			if (dbHelper.DatabaseExists(DbNumber))
			{
				var dbName = dbHelper.GetDatabaseName(DbNumber);
				dbHelper.DropDatabase(dbName);
			}
		}

		const int DbNumber = 900;
		readonly DocManagerDBHelperTestClass dbHelper = new();
	}

	class TestArchiveSchedule : IArchiveSchedule
	{
		public void AttachEDoc(string filepath, string eDocFileName, string docType, string description)
		{
			return;
		}

		public IArchiveWatermark GetWatermark(string stageName)
		{
			if (!WatermarkDictionary.TryGetValue(stageName, out var result))
			{
				result = null;
			}

			return result;
		}

		public void SetWatermark(string stageName, IArchiveWatermark watermark)
		{
			if (WatermarkDictionary.ContainsKey(stageName))
			{
				WatermarkDictionary[stageName] = watermark;
			}
			else
			{
				WatermarkDictionary.Add(stageName, watermark);
			}
		}

		readonly Dictionary<string, IArchiveWatermark> WatermarkDictionary = new();

		readonly Guid schedulePK = Guid.Empty;

		public Guid SchedulePK => schedulePK;
	}
}
