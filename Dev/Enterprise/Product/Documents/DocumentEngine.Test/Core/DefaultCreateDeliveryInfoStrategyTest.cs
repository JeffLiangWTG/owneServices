using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DefaultCreateDeliveryInfoStrategyTest : TestCaseWithFactory
	{
		public void TestDeliveryInfoGroupShouldRespectContactDeliveryGroup()
		{
			using (var pack = new DocumentPack(Factory.New<ReportCommand>()))
			using (var dummyReport = new Report(pack, TestReport, new DataProviderList(new DummyDataSource()), "New Report 1", null, DocumentDirection.ANY, false))
			{
				var strategy = new DefaultCreateDeliveryInfoStrategy();
				var instructions = new DeliveryInstructions();
				var contact = new DocDeliveryContact(Factory) { EmailSubjectMacro = "Subject" };
				var groupId = ZGuid.NewZGuid();
				contact.DeliveryGroupId = groupId;

				var deliveryInfo = strategy.CreateDeliveryInfo(dummyReport, contact, instructions);
				AssertEquals(groupId, deliveryInfo.DeliveryGroupID);
			}
		}

		public void TestCreateDeliveryInfoWithMultipleDeliveryGroups()
		{
			var strategy = new DefaultCreateDeliveryInfoStrategy();
			var instructions = new DeliveryInstructions();
			instructions.DeliveryGroups.Add(Factory.New<StmDeliveryGroup>());

			using (var pack = new DocumentPack(Factory.New<ReportCommand>()))
			using (var dummyReport = new Report(pack, TestReport, new DataProviderList(new DummyDataSource()), "New Report 1", null, DocumentDirection.ANY, false))
			{
				((IDeliverable)dummyReport).DeliveryGroupID = instructions.DeliveryGroups[1].PK;
				var deliveryInfo = strategy.CreateDeliveryInfo(dummyReport, new DocDeliveryContact(Factory), instructions);

				AssertEquals("Delivery group retrieved", instructions.DeliveryGroups[1].PK, deliveryInfo.DeliveryGroupID);
			}
		}

		public void TestCreateDeliveryInfoWithEmailSubjectOverride()
		{
			var strategy = new DefaultCreateDeliveryInfoStrategy();
			var instructions = new DeliveryInstructionWithEmailSubjectOverrideForTest { EmailSubjectOverride = "ABC" };

			using (var pack = new DocumentPack(Factory.New<ReportCommand>()))
			using (var dummyReport = new Report(pack, TestReport, new DataProviderList(new DummyDataSource()), "New Report 1", null, DocumentDirection.ANY, false))
			{
				var deliveryInfo = strategy.CreateDeliveryInfo(dummyReport, new DocDeliveryContact(Factory), instructions);

				AssertEquals("EmailSubjectLine Overridden", "ABC", deliveryInfo.EmailSubjectLine);
			}
		}

		public void TestCreateDeliveryInfoWithMultipleDeliveryGroupsAndEDocs()
		{
			var strategy = new DefaultCreateDeliveryInfoStrategy();
			var instructions = new DeliveryInstructions();
			instructions.DeliveryGroups.Add(Factory.New<StmDeliveryGroup>());

			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var storageMain = documentFactory.New<IStorageMain>();
			using (var streamContents = (SubStreamableStream)new MemoryStream(new byte[] { 0x44, 0x65, 0x78, 0x74, 0x65, 0x72 }))
			{
				var eDoc = storageMain.AddFileOrDocument(streamContents, "Dexter", "MSC", true);
				eDoc.SetValuesForTest(new ZDateTime(2009, 09, 08, 08, 56, 0), "TXT");
				((IDeliverable)eDoc).DeliveryGroupID = instructions.DeliveryGroups[1].PK;

				var deliveryInfo = strategy.CreateDeliveryInfo((IDeliverable)eDoc, new DocDeliveryContact(Factory), instructions);
				AssertEquals("Delivery group retrieved", instructions.DeliveryGroups[1].PK, deliveryInfo.DeliveryGroupID);
			}
		}

		public void TestCreateDeliveryInfoWithDefaultDeliveryGroup()
		{
			var strategy = new DefaultCreateDeliveryInfoStrategy();
			var instructions = new DeliveryInstructions();

			using (var pack = new DocumentPack(Factory.New<ReportCommand>()))
			using (var dummyReport = new Report(pack, TestReport, new DataProviderList(new DummyDataSource()), "New Report 1", null, DocumentDirection.ANY, false))
			{
				var deliveryInfo = strategy.CreateDeliveryInfo(dummyReport, new DocDeliveryContact(Factory), instructions);

				AssertEquals("Default delivery group retrieved", instructions.DeliveryGroups[0].PK, deliveryInfo.DeliveryGroupID);
			}
		}

		public void TestCreateDeliveryInfoWithEmailFromAddressOverride()
		{
			var strategy = new DefaultCreateDeliveryInfoStrategy();
			var instructions = new DeliveryInstructions();
			GlbStaff.CurrentUser.GS_EmailAddress = "Main@test.com";
			var contact = new DocDeliveryContact(Factory)
			{
				EmailFromAddress = "Main@test.com",
				DeliveryMethod = Core.Constants.ContactNotifyModes.Email
			};

			using (var pack = new DocumentPack(Factory.New<ReportCommand>()))
			using (var dummyReport = new Report(pack, TestReport, new DataProviderList(new DummyDataSource()), "New Report 1", null, DocumentDirection.ANY, false))
			{
				var deliveryInfo = strategy.CreateDeliveryInfo(dummyReport, contact, instructions);

				Assert(!contact.EmailFromAddressInfo.HasErrors());
				AssertEquals("EmailFromAddress Overridden", "Main@test.com", deliveryInfo.EmailFromAddress);
			}
		}

		public void TestCreateDeliveryInfoWithPDFPasswordFromDocumentCommand()
		{
			using (Report.TemporarilyUseMainConnection())
			{
				var strategy = new DefaultCreateDeliveryInfoStrategy();
				var instructions = new DeliveryInstructions();

				var encryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("password");

				GlbStaff.CurrentUser.GS_EmailAddress = "Main@test.com";
				var contact = new DocDeliveryContact(Factory)
				{
					EmailFromAddress = "Main@test.com",
					DeliveryMethod = Core.Constants.ContactNotifyModes.Email
				};

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_MenuDataContext = nameof(Core.Constants.DataContext.UnitTest);
				documentCommand.Parent = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();

				using (var pack = new DocumentPack(documentCommand))
				using (var dummyReport = new Report(pack: pack,
										template: TestReport,
										docDataProvider: new DataProviderList(new DummyDataSource()),
										reportName: "New Report 1",
										userDefinedFieldValueList: null,
										direction: DocumentDirection.ANY,
										isPasswordProtectedForModifying: false))
				{
					var deliveryInfo = strategy.CreateDeliveryInfo(dummyReport, contact, instructions);
					AssertEquals("report with no documentSupportable bizo would not have PDF password provided.", encryptedPassword, deliveryInfo.PDFEncryptionPassword);
				}

				documentCommand.Parent = null;

				using (var pack = new DocumentPack(documentCommand))
				using (var dummyReport = new Report(pack: pack,
										template: TestReport,
										docDataProvider: new DataProviderList(new DummyDataSource()),
										reportName: "New Report 1",
										userDefinedFieldValueList: null,
										direction: DocumentDirection.ANY,
										isPasswordProtectedForModifying: false))
				{
					var deliveryInfo = strategy.CreateDeliveryInfo(dummyReport, contact, instructions);
					AssertNullOrEmpty("report with no documentSupportable bizo would not have PDF password provided.", deliveryInfo.PDFEncryptionPassword);
				}
			}
		}

		public void TestRunScheduledReportAsExcelFileWithEmailNotificationWhenEmpty()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Hello World";

			var reportCommand = Factory.New<ReportCommand>();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (Report.TemporarilyUseMainConnection())
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = ContactNotifyModes.Email;
				recipient.AttachmentType = AttachmentTypeList.Codes.Xls;
				recipient.Email = @"unit.test@cargowise.com";

				var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);

				AssertEquals("Pre-condition: There should be one recipient.", 1, scheduledReport.Recipients.Count);

				var scheduledReportRecipient = scheduledReport.Recipients[0];
				scheduledReportRecipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
				scheduledReport.Run();

				var query = new ZDBOnlyQuery(typeof(StmPrintJob));
				var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
				subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
				subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, @"unit.test@cargowise.com");
				query.AddSubQuery(subQuery, JoinCondition.And);

				var printJobs = Factory.Load<StmPrintJob>(query);

				AssertEquals("There should be one print job created.", 1, printJobs.Length);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);

					AssertMultilineASCIIEquals(
						"There should be one row in this report.",
						@"{B}-[Hello World]",
						excelInterface.WorkSheets[0].ToString());
				}
			}
		}

		public void TestDocumentTranslateLegacyDocumentIsSyncedFromInstructions()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[#SectionBody]
{B}-[TestCell]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				var template = Factory.New<StmTemplate>();
				template.SO_IsSystemDefined = true;
				template.SO_Name = "LegacyDocument";
				template.SO_ExcelTemplatePath = report.Template.TemplateSourceLocation;
				report.StTemplate = template;

				var instructions = new DeliveryInstructions(documentPack);
				var defaultCreateDeliveryInfoStrategy = new DefaultCreateDeliveryInfoStrategy();
				defaultCreateDeliveryInfoStrategy.CreateDeliveryInfo(report, new DocDeliveryContact(Factory), instructions);
				AssertEquals("legacy document should be translated", true, report.TranslateLegacyDocument);
			}
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		ExcelTemplateForUnitTesting testReport;
		ExcelTemplateForUnitTesting TestReport
		{
			get
			{
				if (testReport == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					testReport = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return testReport;
			}
		}
	}
}
