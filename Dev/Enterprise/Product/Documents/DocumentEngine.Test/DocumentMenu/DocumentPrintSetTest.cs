using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentPrintSetTest : PrintTaskDocumentPackLoaderTestCase
	{
		public void TestDocumentPrintSetShouldShowProgressBar()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = string.Empty;
			documentCommand.SU_EmailSubjectLine = "<CompanyName>";

			using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Name = value, GlbCompany.CurrentCompany.GC_Name, "<AAA>"))
			using (var documentPrintSet = new DocumentPrintSet(documentCommand, null))
			{
				AssertEquals("Settings_StartDocumentGeneration\nSettings_EndDocumentGeneration\n", documentPrintSet.TaskSettings.NotificationLogs);
			}
		}

		public void TestDeliveryGroupEmailSubjectHasAngleBracket()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = string.Empty;
			documentCommand.SU_EmailSubjectLine = "<CompanyName>";

			using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Name = value, GlbCompany.CurrentCompany.GC_Name, "<AAA>"))
			using (var documentPrintSet = new DocumentPrintSet(documentCommand, null))
			{
				var pack = new DocumentPack();
				var dummyReport = new Report(pack, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "New Report", null, DocumentDirection.ANY, false);

				pack.Add(dummyReport);
				documentPrintSet.Add(pack);

				var deliveryInstructions = new DeliveryInstructions { Destination = DeliveryInstructionDestination.Auto };
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.Email = "test@test.com";
				var group = deliveryInstructions.DeliveryGroups[0];

				documentPrintSet.Run(deliveryInstructions);
				AssertEquals("deliveryInstructions.DeliveryGroup.SB_EmailSubjectLine", "<AAA>", group.SB_EmailSubjectLine);
			}
		}

		public void TestEmailSubjectContextShouldFallBackToFirstTemplateIfNotEntered()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = string.Empty;

			using (var documentPrintSet = new DocumentPrintSet(documentCommand, null))
			{
				var pack = new DocumentPack();
				var dummyReport = new Report(pack, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "New Report", null, DocumentDirection.ANY, false);

				pack.Add(dummyReport);
				documentPrintSet.Add(pack);

				var deliveryInstructions = new DeliveryInstructions { Destination = DeliveryInstructionDestination.Auto };
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.Email = "test@test.com";
				var group = deliveryInstructions.DeliveryGroups[0];

				documentCommand.SU_EmailSubjectLine = "<Name>";
				documentPrintSet.Run(deliveryInstructions);
				AssertEquals("deliveryInstructions.DeliveryGroup.SB_EmailSubjectLine", "Earl", group.SB_EmailSubjectLine);
			}
		}

		[TestDate(2010, 05, 24, 10, 10, 00)]
		public void TestMultipleNestedMacrosInEmailSubjectOverride()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = string.Empty;

			using (var documentPrintSet = new DocumentPrintSet(documentCommand, null))
			{
				var pack = new DocumentPack();
				var dummyReport = new Report(pack, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "New Report", null, DocumentDirection.ANY, false);

				pack.Add(dummyReport);
				documentPrintSet.Add(pack);

				var deliveryInstructions = new DeliveryInstructions { Destination = DeliveryInstructionDestination.Auto };
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.Email = "test@test.com";
				var group = deliveryInstructions.DeliveryGroups[0];

				documentCommand.SU_EmailSubjectLine = "Current Date: <DateTimeAsString('<Now>', 'dd-MMM-yy')> Current Time: <DateTimeAsString('<Now>', 'hh:mm')>";
				documentPrintSet.Run(deliveryInstructions);

				AssertEquals("deliveryInstructions.DeliveryGroup.SB_EmailSubjectLine", "Current Date: 24-May-10 Current Time: 10:10", group.SB_EmailSubjectLine);
			}
		}

		public void TestEmailSubjectLineMacrosShouldBeRemovedIfDataContextDoesNotExist()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = nameof(Core.Constants.DataContext.UnitTest);
			documentCommand.Parent = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();

			using (var documentPrintSet = new DocumentPrintSet(documentCommand, null))
			{
				var pack = new DocumentPack();
				var dummyReport = new Report(pack, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "New Report", null, DocumentDirection.ANY, false);

				pack.Add(dummyReport);
				documentPrintSet.Add(pack);

				var deliveryInstructions = new DeliveryInstructions { Destination = DeliveryInstructionDestination.TakenFromContact };
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.Email = "test@test.com";

				var group = deliveryInstructions.DeliveryGroups[0];

				documentCommand.SU_EmailSubjectLine = "<SU_EmailSubjectLine>";
				documentPrintSet.Run(deliveryInstructions);
				AssertEquals("deliveryInstructions.DeliveryGroup.SB_EmailSubjectLine", ZString.Empty, group.SB_EmailSubjectLine);

				documentCommand.SU_EmailSubjectLine = "Test<SU_EmailSubjectLine>";
				documentPrintSet.Run(deliveryInstructions);
				AssertEquals("deliveryInstructions.DeliveryGroup.SB_EmailSubjectLine", "Test", group.SB_EmailSubjectLine);
			}
		}

		public void TestEmailSubjectLineShouldNotBeInterruptedByCoverSheet()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = string.Empty;

			using (var documentPrintSet = new DocumentPrintSet(documentCommand, null))
			{
				var pack = new DocumentPack();
				var dummyReport = new Report(pack, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Justin Chen")), "New Report", null, DocumentDirection.ANY, false);

				var deliveryInstructions = new DeliveryInstructions
				{
					Destination = DeliveryInstructionDestination.Auto
				};
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.Email = "test@test.com";
				var group = deliveryInstructions.DeliveryGroups[0];
				var coverSheet = new Report(pack,
					new ExcelTemplateReadFromExcelTemplatesSolution(
						ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.EmailCoverSheet), deliveryInstructions,
					"Email Cover Sheet", null, DocumentDirection.ANY, false)
				{
					PrintCopyType = PrintCopyType.EML,
					IsCoverSheet = true
				};

				pack.Add(coverSheet);
				pack.Add(dummyReport);
				documentPrintSet.Add(pack);

				documentCommand.SU_EmailSubjectLine = "<Name>";
				documentPrintSet.Run(deliveryInstructions);
				AssertEquals("deliveryInstructions.DeliveryGroup.SB_EmailSubjectLine", "Justin Chen", group.SB_EmailSubjectLine);
			}
		}

		public void TestContainsReports()
		{
			var command = Factory.New<DocumentCommand>();
			var set = new DocumentPrintSet(command, null);

			AssertEquals(false, set.ContainsReports());

			var pack = new DocumentPack();
			set.Add(pack);
			AssertEquals(false, set.ContainsReports());

			pack.Add(new DummyDeliverable());
			AssertEquals(false, set.ContainsReports());

			pack.Add(new Report(pack, null));
			AssertEquals(true, set.ContainsReports());
		}

		public void TestContainsReportsForFormBuilderDocs()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			var documentPrintSet = new DocumentPrintSet(documentCommand, null);
			var documentPack = new DocumentPack();
			documentPrintSet.Add(documentPack);

			AssertEquals("Precondition", false, documentPrintSet.ContainsReports());

			var dummyDeliverable = new DummyDeliverable();
			dummyDeliverable.MenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			dummyDeliverable.MenuItem.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;

			documentPack.Add(dummyDeliverable);

			AssertEquals("Document packs should treat Form Builder documents as Reports.", true, documentPrintSet.ContainsReports());
		}

		public void TestContainsReportsForEDocs()
		{
			var command = Factory.New<DocumentCommand>();
			var printSet = new DocumentPrintSet(command, null);

			AssertEquals(false, printSet.ContainsReports());

			var storageDocs = Factory.New<DummyBizoStorageDocs>();

			var pack = new DocumentPack();
			printSet.Add(pack);
			AssertEquals(false, printSet.ContainsReports());

			pack.Add(storageDocs);
			AssertEquals(true, printSet.ContainsReports());
		}

		public void TestContainsReportsForEDocsWithStorageFile()
		{
			var command = Factory.New<DocumentCommand>();
			var printSet = new DocumentPrintSet(command, null);

			AssertEquals(false, printSet.ContainsReports());

			var storageFile = Factory.New<DummyBizoStorageFile>();

			var pack = new DocumentPack();
			printSet.Add(pack);
			AssertEquals(false, printSet.ContainsReports());

			pack.Add(storageFile);
			AssertEquals(true, printSet.ContainsReports());
		}

		public void TestAvailablePrintOptionsWhenNoTemplates()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			using (DummyDocumentPrintSet printSet = new DummyDocumentPrintSet(command, new UserControlProviderList()))
			{
				DeliveryInstructions instructions = new DeliveryInstructions();

				printSet.CheckAvailableDeliveryOptions(instructions);
				Assert("Should allow print", instructions.AllowPrint);
				Assert("Should allow email", instructions.AllowEmail);
				Assert("Should allow fax", instructions.AllowFax);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAvailablePrintOptions()
		{
			#region Templates

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			template.SO_IsSystemDefined = true;
			template.SO_Name = "System Shipment Template";
			ExcelTemplateForUnitTesting shipmentTemplate = new ExcelTemplateForUnitTesting("UDF with tabs3.xls", TestFilesSubFolder.ReportTestFiles);
			template.SO_Template = shipmentTemplate.GetAsByteArray();

			#endregion

			#region Published System Shipment

			DocumentCommand command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command.SU_IsPublished = true;
			command.SU_IsSystemDefined = true;
			command.SU_MenuName = "Pub System Shipment Document";
			command.SU_MenuIndex = 1;
			command.SU_MenuPath = "";
			command.SU_MenuShortcut = "CtrlF1";
			command.SU_ContactType = ContactType.Consignee.ToString();

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = command.PK;
			pivot.SI_DocumentTitle = "Pub System Shipment Document";

			#endregion

			DocumentCommandTest.DocDummyBusinessObject bizO = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			command.Parent = bizO;

			using (DummyDocumentPrintSet printSet = new DummyDocumentPrintSet(command, new UserControlProviderList()))
			{
				DeliveryInstructions instructions = new DeliveryInstructions();

				pivot.SI_PrintCopyType = "";
				printSet.CheckAvailableDeliveryOptions(instructions);
				Assert("Should allow print", instructions.AllowPrint);
				Assert("Should allow email", instructions.AllowEmail);
				Assert("Should allow fax", instructions.AllowFax);

				pivot.SI_PrintCopyType = nameof(PrintCopyType.EML);
				printSet.CheckAvailableDeliveryOptions(instructions);
				Assert("Should not allow print", !instructions.AllowPrint);
				Assert("Should allow email", instructions.AllowEmail);
				Assert("Should not allow fax", !instructions.AllowFax);

				pivot.SI_PrintCopyType = nameof(PrintCopyType.FAX);
				printSet.CheckAvailableDeliveryOptions(instructions);
				Assert("Should not allow print", !instructions.AllowPrint);
				Assert("Should not allow email", !instructions.AllowEmail);
				Assert("Should allow fax", instructions.AllowFax);

				pivot.SI_PrintCopyType = nameof(PrintCopyType.PRN);
				printSet.CheckAvailableDeliveryOptions(instructions);
				Assert("Should allow print", instructions.AllowPrint);
				Assert("Should not allow email", !instructions.AllowEmail);
				Assert("Should not allow fax", !instructions.AllowFax);

				pivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
				printSet.CheckAvailableDeliveryOptions(instructions);
				Assert("Should allow print", instructions.AllowPrint);
				Assert("Should allow email", instructions.AllowEmail);
				Assert("Should allow fax", instructions.AllowFax);
			}
		}

		public void TestGenerateSubjectLineForDocumentPrintSetWithNoReplacements()
		{
			var testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "TestCommand";
			testCommand.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			testCommand.SU_EmailSubjectLine = "This is a test email subject line with no replacements";

			var printSet = new DummyDocumentPrintSet(testCommand, new UserControlProviderList());
			PrintTask.ReportSubjectLineMapping reportSubjectMapping = printSet.GenerateSubjectLineMappingForPrintTaskForTesting(DocumentPack.EmptyPack);

			Assert("Subject line should not be empty", !reportSubjectMapping.SubjectLine.IsEmpty);
			Assert("Subject line should have all instances of '<' removed", !reportSubjectMapping.SubjectLine.Contains('<'));
			Assert("Subject line should have all instances of '>' removed", !reportSubjectMapping.SubjectLine.Contains('>'));
			Assert("Subject line should have all newline chars removed", !reportSubjectMapping.SubjectLine.Contains("\n"));
			AssertEquals("Subject line after replacements", "This is a test email subject line with no replacements", reportSubjectMapping.SubjectLine);
		}

		public void TestGenerateSubjectLineForDocumentPrintSetWithNoValue()
		{
			var testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "TestCommand";
			testCommand.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			testCommand.SU_EmailSubjectLine = "";

			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			testCommand.Parent = dummy;

			var printSet = new DummyDocumentPrintSet(testCommand, new UserControlProviderList());
			PrintTask.ReportSubjectLineMapping reportSubjectMapping = printSet.GenerateSubjectLineMappingForPrintTaskForTesting(DocumentPack.EmptyPack);

			Assert("Subject line should not be empty", !reportSubjectMapping.SubjectLine.IsEmpty);
			Assert("Subject line should have all instances of '<' removed", !reportSubjectMapping.SubjectLine.Contains('<'));
			Assert("Subject line should have all instances of '>' removed", !reportSubjectMapping.SubjectLine.Contains('>'));
			Assert("Subject line should have all newline chars removed", !reportSubjectMapping.SubjectLine.Contains("\n"));
			AssertEquals("Subject line after replacements - because data context is set, it will use the company name, branch, menu name and the datacontextwrapper.tostring() to get the subject line",
			GlbCompany.CurrentCompany.GC_Name.ToString() + " (" + GlbBranch.CurrentBranch.GB_BranchName.ToString() + ") - TestCommand - ", reportSubjectMapping.SubjectLine);
		}

		public void TestGenerateSubjectLineForDocumentPrintSetWithNoValue_CaterForDeliveryGroups()
		{
			var testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "TestCommand";
			testCommand.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			testCommand.SU_EmailSubjectLine = string.Empty;

			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			testCommand.Parent = dummy;

			using (var printSet = new DummyDocumentPrintSet(testCommand, new UserControlProviderList()))
			{
				var org1 = OrgHeader.New(Factory);
				var contact1 = org1.Contacts.AddNew();
				contact1.OC_ContactName = "Test Contact 1";
				contact1.OC_Email = "dexter@morgan.com";

				var org2 = OrgHeader.New(Factory);
				var contact2 = org2.Contacts.AddNew();
				contact2.OC_ContactName = "Test Contact 2";
				contact2.OC_Email = "debra@morgan.com";

				var instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Print;
				instructions.DocumentPackCount = 2;

				var pack1 = new DocumentPack();
				pack1.OrgHeaderContact = new OrgHeaderContact(contact1);
				var dummyReport1 = new Report(pack1, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Report1")), "New Report 1", null, DocumentDirection.ANY, false);
				pack1.Add(dummyReport1);

				var pack2 = new DocumentPack();
				pack2.OrgHeaderContact = new OrgHeaderContact(contact2);
				var dummyReport2 = new Report(pack2, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Report2")), "New Report 2", null, DocumentDirection.ANY, false);
				pack2.Add(dummyReport2);

				printSet.Add(pack1);
				printSet.Add(pack2);
				printSet.Run(instructions);

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Two print jobs should be there", 2, printJobs.Length);
				AssertNotEquals("Print jobs should have different delivery groups", printJobs[0].SP_SB_DeliveryGroup, printJobs[1].SP_SB_DeliveryGroup);
			}
		}

		public void TestGenerateSubjectLineForDocumentPrintSetWithNoValueAndNoContext()
		{
			var testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "TestCommand";
			testCommand.SU_MenuDataContext = "";
			testCommand.SU_EmailSubjectLine = "";
			testCommand.SU_IsDocPack = true;

			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			testCommand.Parent = dummy;

			var printSet = new DummyDocumentPrintSet(testCommand, new UserControlProviderList());
			PrintTask.ReportSubjectLineMapping reportSubjectLineMapping = printSet.GenerateSubjectLineMappingForPrintTaskForTesting(DocumentPack.EmptyPack);

			AssertEquals("Should be the EmptyMapping", PrintTask.ReportSubjectLineMapping.EmptyMapping, reportSubjectLineMapping);
		}

		public void TestGenerateSubjectLineForDocumentPrintSetWithUDFReplacements()
		{
			var testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "TestCommand";
			testCommand.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			testCommand.SU_EmailSubjectLine = "This is a test email subject line <Field1> <TestInt> with newline chars \n\n";

			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			testCommand.Parent = dummy;

			var udfList = new UserControlProviderList();
			var field1 = new TextField(new BusinessObjectFactory());
			field1.DisplayName = "Field1";
			field1.Value = "Value1";

			var field2 = new TextField(new BusinessObjectFactory());
			field2.DisplayName = "TestInt";
			field2.Value = "1";

			udfList.Add(field1);
			udfList.Add(field2);

			using (var printSet = new DummyDocumentPrintSet(testCommand, udfList))
			{
				PrintTask.ReportSubjectLineMapping reportSubjectMapping = printSet.GenerateSubjectLineMappingForPrintTaskForTesting(DocumentPack.EmptyPack);

				Assert("Subject line should not be empty", !reportSubjectMapping.SubjectLine.IsEmpty);
				Assert("Subject line should have all instances of '<' removed", !reportSubjectMapping.SubjectLine.Contains('<'));
				Assert("Subject line should have all instances of '>' removed", !reportSubjectMapping.SubjectLine.Contains('>'));
				Assert("Subject line should have all newline chars removed", !reportSubjectMapping.SubjectLine.Contains("\n"));
				AssertEquals("Subject line after replacements", "This is a test email subject line Value1 1 with newline chars   ", reportSubjectMapping.SubjectLine);
			}
		}

		public void TestReportSubjectLineMappingWithDocPackSupportConsolidatedEmailSubject()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			EmailFormat emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "TestCommand";
			testCommand.SU_EmailSubjectLine = "Name - <Name>";
			testCommand.SU_IsDocPack = false;
			DocumentCommandTest.DocDummyBusinessObject dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			testCommand.Parent = dummy;

			using (DummyDocumentPrintSet printSet = new DummyDocumentPrintSet(testCommand, new UserControlProviderList()))
			{
				OrgHeader org1 = OrgHeader.New(Factory);
				OrgContact contact1 = org1.Contacts.AddNew();
				contact1.OC_ContactName = "Test Contact 1";
				contact1.OC_Email = "dexter@morgan.com";

				OrgHeader org2 = OrgHeader.New(Factory);
				OrgContact contact2 = org2.Contacts.AddNew();
				contact2.OC_ContactName = "Test Contact 2";
				contact2.OC_Email = "debra@morgan.com";

				DeliveryInstructions instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Auto;
				instructions.DocumentPackCount = 2;

				DocumentPack pack1 = new DocumentPack(testCommand, null, null, null);
				pack1.OrgHeaderContact = new OrgHeaderContact(contact1);

				Report dummyReport1_1 = new Report(pack1, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "New Report 1_1", null, DocumentDirection.ANY, false);
				pack1.Add(dummyReport1_1);
				Report dummyReport1_2 = new Report(pack1, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "New Report 1_2", null, DocumentDirection.ANY, false);
				pack1.Add(dummyReport1_2);
				pack1.NumberOfDocumentNeedToBeConsolidated = 2;
				pack1.EmailSubjectForConsolidateReports = "Hello world";

				DocumentPack pack2 = new DocumentPack(Factory.New<ReportCommand>());
				pack2.OrgHeaderContact = new OrgHeaderContact(contact2);
				Report dummyReport2 = new Report(pack2, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Billy Bob")), "New Report 2", null, DocumentDirection.ANY, false);
				pack2.Add(dummyReport2);

				printSet.Add(pack1);
				printSet.Add(pack2);

				printSet.Run(instructions);

				Assert("Subject lines should not be empty", !printSet.SubjectLineMappingsForPrintTaskForTesting[0].SubjectLine.IsEmpty);
				Assert("Subject lines should not be empty", !printSet.SubjectLineMappingsForPrintTaskForTesting[1].SubjectLine.IsEmpty);
				AssertEquals("Report should be mapped", dummyReport1_1, printSet.SubjectLineMappingsForPrintTaskForTesting[0].ReportWithDeliverables.Report);
				AssertEquals("Report should be mapped", dummyReport2, printSet.SubjectLineMappingsForPrintTaskForTesting[1].ReportWithDeliverables.Report);
				AssertEquals("Mapped subject line", "Hello world", printSet.SubjectLineMappingsForPrintTaskForTesting[0].SubjectLine);
				AssertEquals("Mapped subject line", "Name - Billy Bob", printSet.SubjectLineMappingsForPrintTaskForTesting[1].SubjectLine);
			}

			StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Two print jobs should be there", 2, printJobs.Length);

			List<StmPrintJob> printJobsForDexter = printJobs.Where(job => job.SP_Destination == "dexter@morgan.com").ToList();
			AssertEquals("One print job should be there for Dexter", 1, printJobsForDexter.Count);
			AssertEquals("Email Subject correctly set", "Hello world", printJobsForDexter[0].SP_EmailSubjectLine);

			List<StmPrintJob> printJobsForDebra = printJobs.Where(job => job.SP_Destination == "debra@morgan.com").ToList();
			AssertEquals("One print job should be there for Debra", 1, printJobsForDebra.Count);
			AssertEquals("Email Subject correctly set", "New Report 2 - Billy Bob", printJobsForDebra[0].SP_EmailSubjectLine);

			AssertNotEquals("Dexter's and Debra's print jobs should not have the same DeliveryGroupId", printJobsForDexter[0].SP_SB_DeliveryGroup, printJobsForDebra[0].SP_SB_DeliveryGroup);
		}

		public void TestReportSubjectLineMappingWithMultiplePacks()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			EmailFormat emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "TestCommand";
			testCommand.SU_EmailSubjectLine = "Name - <Name>";
			testCommand.SU_IsDocPack = false;
			DocumentCommandTest.DocDummyBusinessObject dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			testCommand.Parent = dummy;

			using (DummyDocumentPrintSet printSet = new DummyDocumentPrintSet(testCommand, new UserControlProviderList()))
			{
				OrgHeader org1 = OrgHeader.New(Factory);
				OrgContact contact1 = org1.Contacts.AddNew();
				contact1.OC_ContactName = "Test Contact 1";
				contact1.OC_Email = "dexter@morgan.com";

				OrgHeader org2 = OrgHeader.New(Factory);
				OrgContact contact2 = org2.Contacts.AddNew();
				contact2.OC_ContactName = "Test Contact 2";
				contact2.OC_Email = "debra@morgan.com";

				DeliveryInstructions instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Auto;
				instructions.DocumentPackCount = 2;

				DocumentPack pack1 = new DocumentPack(Factory.New<ReportCommand>());
				pack1.OrgHeaderContact = new OrgHeaderContact(contact1);
				Report dummyReport1 = new Report(pack1, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "New Report 1", null, DocumentDirection.ANY, false);
				pack1.Add(dummyReport1);

				DocumentPack pack2 = new DocumentPack(Factory.New<ReportCommand>());
				pack2.OrgHeaderContact = new OrgHeaderContact(contact2);
				Report dummyReport2 = new Report(pack2, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Billy Bob")), "New Report 2", null, DocumentDirection.ANY, false);
				pack2.Add(dummyReport2);

				printSet.Add(pack1);
				printSet.Add(pack2);

				printSet.Run(instructions);

				Assert("Subject lines should not be empty", !printSet.SubjectLineMappingsForPrintTaskForTesting[0].SubjectLine.IsEmpty);
				Assert("Subject lines should not be empty", !printSet.SubjectLineMappingsForPrintTaskForTesting[1].SubjectLine.IsEmpty);
				AssertEquals("Report should be mapped", dummyReport1, printSet.SubjectLineMappingsForPrintTaskForTesting[0].ReportWithDeliverables.Report);
				AssertEquals("Report should be mapped", dummyReport2, printSet.SubjectLineMappingsForPrintTaskForTesting[1].ReportWithDeliverables.Report);
				AssertEquals("Mapped subject line", "Name - Earl", printSet.SubjectLineMappingsForPrintTaskForTesting[0].SubjectLine);
				AssertEquals("Mapped subject line", "Name - Billy Bob", printSet.SubjectLineMappingsForPrintTaskForTesting[1].SubjectLine);
			}

			StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Two print jobs should be there", 2, printJobs.Length);

			List<StmPrintJob> printJobsForDexter = printJobs.Where(job => job.SP_Destination == "dexter@morgan.com").ToList();
			AssertEquals("One print job should be there for Dexter", 1, printJobsForDexter.Count);
			AssertEquals("Email Subject correctly set", "New Report 1 - Earl", printJobsForDexter[0].SP_EmailSubjectLine);

			List<StmPrintJob> printJobsForDebra = printJobs.Where(job => job.SP_Destination == "debra@morgan.com").ToList();
			AssertEquals("One print job should be there for Debra", 1, printJobsForDebra.Count);
			AssertEquals("Email Subject correctly set", "New Report 2 - Billy Bob", printJobsForDebra[0].SP_EmailSubjectLine);

			AssertNotEquals("Dexter's and Debra's print jobs should not have the same DeliveryGroupId", printJobsForDexter[0].SP_SB_DeliveryGroup, printJobsForDebra[0].SP_SB_DeliveryGroup);
		}

		public void TestIsDraft_SU_EmailSubjectLine()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = "";
			documentCommand.SU_EmailSubjectLine = "<If(\"<IsDraft>\"==\"Y\",\"DRAFT Arrival Notice\",\"<ReportName>\")>";
			documentCommand.SU_IsDocPack = false;
			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			documentCommand.Parent = dummy;

			using (var testTask = new DocumentPrintSet(documentCommand, new UserControlProviderList()))
			{
				var org1 = OrgHeader.New(Factory);
				var contact1 = org1.Contacts.AddNew();
				contact1.OC_ContactName = "Test Contact 1";
				contact1.OC_Email = "dexter@morgan.com";

				var instructions = new DeliveryInstructions();
				instructions.IsDraft = true;
				instructions.Destination = DeliveryInstructionDestination.Auto;
				instructions.DocumentPackCount = 1;

				var testPack1 = new DocumentPack();
				testPack1.OrgHeaderContact = new OrgHeaderContact(contact1);
				var dummyReport1 = new Report(testPack1, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Report1")), "New Report 1", null, DocumentDirection.ANY, false);
				testPack1.Add(dummyReport1);

				testTask.Add(testPack1);
				testTask.Run(instructions);
				AssertEquals("DRAFT Arrival Notice", instructions.DeliveryGroups[0].SB_EmailSubjectLine);
			}
		}

		public void TestCreateReportSubjectLineMappingsWithMultiplePacksAndEdocs()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuDataContext = "";
			documentCommand.SU_EmailSubjectLine = "<Name>";
			documentCommand.SU_IsDocPack = false;
			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			documentCommand.Parent = dummy;

			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var storageMain = documentFactory.New<IStorageMain>();

			var eDoc1 = storageMain.AddFileOrDocument(new byte[] { 0x44, 0x65, 0x78, 0x74, 0x65, 0x72 }, "Dexter", "MSC", true);
			eDoc1.SetValuesForTest(new ZDateTime(2009, 09, 08, 08, 56, 0), "TXT");
			((IDeliverable)eDoc1).IncludedInPrint = true;
			var eDoc2 = storageMain.AddFileOrDocument(new byte[] { 0x44, 0x65, 0x62, 0x72, 0x61 }, "Debra", "MSC", true);
			eDoc2.SetValuesForTest(new ZDateTime(2009, 09, 08, 08, 56, 0), "TXT");
			((IDeliverable)eDoc2).IncludedInPrint = true;

			using (var testTask = new DocumentPrintSet(documentCommand, new UserControlProviderList()))
			{
				var org1 = OrgHeader.New(Factory);
				var contact1 = org1.Contacts.AddNew();
				contact1.OC_ContactName = "Test Contact 1";
				contact1.OC_Email = "dexter@morgan.com";

				var org2 = OrgHeader.New(Factory);
				var contact2 = org2.Contacts.AddNew();
				contact2.OC_ContactName = "Test Contact 2";
				contact2.OC_Email = "debra@morgan.com";

				var instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Auto;
				instructions.DocumentPackCount = 2;

				var testPack1 = new DocumentPack();
				testPack1.OrgHeaderContact = new OrgHeaderContact(contact1);
				var dummyReport1 = new Report(testPack1, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Report1")), "New Report 1", null, DocumentDirection.ANY, false);
				testPack1.Add(dummyReport1);
				testPack1.Add((IDeliverable)eDoc1);

				var testPack2 = new DocumentPack();
				testPack2.OrgHeaderContact = new OrgHeaderContact(contact2);
				var dummyReport2 = new Report(testPack2, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource("Report2")), "New Report 2", null, DocumentDirection.ANY, false);
				testPack2.Add(dummyReport2);
				testPack2.Add((IDeliverable)eDoc2);

				testTask.Add(testPack1);
				testTask.Add(testPack2);
				testTask.Run(instructions);
			}

			StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Four print jobs should be there", 4, printJobs.Length);

			List<StmPrintJob> printJobsForDexter = printJobs.Where(job => job.SP_Destination == "dexter@morgan.com").ToList();
			AssertEquals("Two print jobs should be there for Dexter", 2, printJobsForDexter.Count);
			AssertEquals("All Dexter's print jobs should have the same DeliveryGroupId", printJobsForDexter[0].SP_SB_DeliveryGroup, printJobsForDexter[1].SP_SB_DeliveryGroup);

			List<StmPrintJob> printJobsForDebra = printJobs.Where(job => job.SP_Destination == "debra@morgan.com").ToList();
			AssertEquals("Two print jobs should be there for Debra", 2, printJobsForDebra.Count);
			AssertEquals("All Debra's print jobs should have the same DeliveryGroupId", printJobsForDexter[0].SP_SB_DeliveryGroup, printJobsForDexter[1].SP_SB_DeliveryGroup);

			AssertNotEquals("Dexter's and Debra's print jobs should not have the same DeliveryGroupId", printJobsForDexter[0].SP_SB_DeliveryGroup, printJobsForDebra[0].SP_SB_DeliveryGroup);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_NonDocPack_WhenOtherDocEmailSubjectIsSetAndDataContextIsSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_NonDocPack("Shipment EmailSubject", false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_NonDocPack_WhenOtherDocEmailSubjectIsSetAndDataContextIsNotSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_NonDocPack("Shipment EmailSubject", true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_NonDocPack_WhenOtherDocEmailSubjectIsNotSetAndDataContextIsSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_NonDocPack("", false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_NonDocPack_WhenOtherDocEmailSubjectIsNotSetAndDataContextIsNotSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_NonDocPack("", true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_DocPack_WhenOtherDocEmailSubjectIsSetAndDataContextIsSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_DocPack("Shipment EmailSubject", false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_DocPack_WhenOtherDocEmailSubjectIsSetAndDataContextIsNotSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_DocPack("Shipment EmailSubject", true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_DocPack_WhenOtherDocEmailSubjectIsNotSetAndDataContextIsSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_DocPack("", false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportSubjectLineMappingsWithMultiplePacks_DocPack_WhenOtherDocEmailSubjectIsNotSetAndDataContextIsNotSet()
		{
			AssertReportSubjectLineMappings_WithOtherDocuments_DocPack("", true);
		}

		public void TestReportSubjectLineIsTranslated()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put(DocBuilderResourceStrings.ReportNameKeyPrefix + "Test Report", new ResourceStringData(DocBuilderResourceStrings.ReportNameKeyPrefix + "Test Report", "测试报告"));

				TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

				DocumentCommand testCommand = Factory.New<DocumentCommand>();
				testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
				testCommand.SU_IsPublished = true;
				testCommand.SU_IsSystemDefined = true;
				testCommand.SU_MenuName = "TestCommand";
				testCommand.SU_EmailSubjectLine = "<ReportName>!";
				testCommand.SU_IsDocPack = true;
				DocumentCommandTest.DocDummyBusinessObject dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
				testCommand.Parent = dummy;

				using (DummyDocumentPrintSet printSet = new DummyDocumentPrintSet(testCommand, new UserControlProviderList()))
				{
					OrgHeader org = OrgHeader.New(Factory);
					OrgContact contact = org.Contacts.AddNew();
					contact.OC_ContactName = "Test Contact";
					contact.OC_Email = "someone@somewhere.com";

					DeliveryInstructions instructions = new DeliveryInstructions();
					instructions.Destination = DeliveryInstructionDestination.Auto;
					instructions.DocumentPackCount = 1;
					instructions.Language = Core.SharedConstants.Languages.ChineseSimplified;

					DocumentPack pack = new DocumentPack();
					pack.OrgHeaderContact = new OrgHeaderContact(contact);
					Report dummyReport = new Report(pack, EmptyAndValidTemplate, new DataProviderList(new DummyDataSource()), "Test Report", null, DocumentDirection.ANY, false);
					pack.Add(dummyReport);

					printSet.Add(pack);

					printSet.Run(instructions);
					AssertEquals("Mapped subject line", "测试报告!", printSet.SubjectLineMappingsForPrintTaskForTesting[0].SubjectLine);
				}

				var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
				AssertEquals("测试报告!", printJob.DeliveryGroup.SB_EmailSubjectLine);
			}
		}

		// create some mock subject line with replacements
		// create another one without replacements
		// create one with replacements and no menu context (should return blank)
		// create one with neither

		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		protected override PrintTask CreateLoadedPrintTask(DocumentCommand command, UserControlProviderList userFieldList) => new DocumentPrintSet(command, userFieldList);

		ExcelTemplateForUnitTesting emptyAndValidTemplate;
		ExcelTemplateForUnitTesting EmptyAndValidTemplate
		{
			get
			{
				if (emptyAndValidTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return emptyAndValidTemplate;
			}
		}

		DummyDocumentPrintSet CreateLoadedDummyDocumentPrintSet(DocumentCommand command, UserControlProviderList userFieldList) => new DummyDocumentPrintSet(command, userFieldList);

		void AssertReportSubjectLineMappings_WithOtherDocuments_NonDocPack(string otherDocumentEmailSubject, bool withOtherDocNullDataContext)
		{
			TestCaseHelper.ClearTable(StmDeliveryGroup.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			var helper = new IsDocPackTestHelper(this);
			helper.CreateTestData(false);
			helper.ConsolCommand.SU_IsDocPack = false;
			helper.ConsolCommand.SU_EmailSubjectLine = "Consol EmailSubject";
			helper.ShipmentCommand.SU_EmailSubjectLine = otherDocumentEmailSubject;
			if (withOtherDocNullDataContext)
			{
				helper.ShipmentCommand.SU_MenuDataContext = null;
			}
			Factory.Save();

			using (var printTask = CreateLoadedDummyDocumentPrintSet(helper.ConsolCommand, null))
			{
				AssertEquals("Count", 2, printTask.Count);

				var taskSettings = new PrintTaskSettings(printTask);
				taskSettings.Destination = DeliveryInstructionDestination.TakenFromContact;

				AssertEquals("Count", 2, taskSettings.DocPacksDeliveryInstructions.Count);

				var deliveryInstruction0 = taskSettings.DocPacksDeliveryInstructions[0];
				deliveryInstruction0.Recipients.RemoveAndDeleteAll();
				var contact0 = deliveryInstruction0.Recipients.AddNew();
				contact0.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact0.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact0.Email = "dexter@morgan.com";

				var deliveryInstruction1 = taskSettings.DocPacksDeliveryInstructions[1];
				deliveryInstruction1.Recipients.RemoveAndDeleteAll();
				var contact1 = deliveryInstruction1.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact1.Email = "debra@morgan.com";

				printTask.Run(taskSettings);

				AssertEquals("Mapped subject line", "Consol EmailSubject", printTask.SubjectLineMappingsForPrintTaskForTesting[0].SubjectLine);
				if (string.IsNullOrEmpty(otherDocumentEmailSubject) && !withOtherDocNullDataContext)
				{
					AssertEquals("Subject line after replacements - because data context is set, it will use the company name, branch, menu name and the datacontextwrapper.tostring() to get the subject line",
						GlbCompany.CurrentCompany.GC_Name.ToString() + " (" + GlbBranch.CurrentBranch.GB_BranchName.ToString() + ") - Shipment Document Command - ", printTask.SubjectLineMappingsForPrintTaskForTesting[1].SubjectLine);
				}
				else
				{
					AssertEquals("Mapped subject line", otherDocumentEmailSubject, printTask.SubjectLineMappingsForPrintTaskForTesting[1].SubjectLine);
				}
			}

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Two print jobs should be there", 2, printJobs.Length);

			var printJobsForDexter = printJobs.Where(job => job.SP_Destination == "dexter@morgan.com").ToList();
			AssertEquals("One print job should be there for Dexter", 1, printJobsForDexter.Count);
			AssertEquals("Email Subject correctly set", "ConsolDocument", printJobsForDexter[0].SP_EmailSubjectLine);

			var printJobsForDebra = printJobs.Where(job => job.SP_Destination == "debra@morgan.com").ToList();
			AssertEquals("One print job should be there for Debra", 1, printJobsForDebra.Count);
			AssertEquals("Email Subject correctly set", "Shipment Document", printJobsForDebra[0].SP_EmailSubjectLine);

			AssertNotEquals("Dexter's and Debra's print jobs should not have the same DeliveryGroupId", printJobsForDexter[0].SP_SB_DeliveryGroup, printJobsForDebra[0].SP_SB_DeliveryGroup);

			var deliveryGroups = Factory.Load<StmDeliveryGroup>(new ZQuery());
			AssertEquals("Two delivery groups should be there", 2, deliveryGroups.Length);

			var deliveryGroupsForDexter = deliveryGroups.Where(group => group.PK == printJobsForDexter[0].SP_SB_DeliveryGroup).ToList();
			AssertEquals("One group should be there for Dexter", 1, deliveryGroupsForDexter.Count);
			AssertEquals("Email Subject correctly set", "Consol EmailSubject", deliveryGroupsForDexter[0].SB_EmailSubjectLine);

			var deliveryGroupsForDebra = deliveryGroups.Where(group => group.PK == printJobsForDebra[0].SP_SB_DeliveryGroup).ToList();
			AssertEquals("One group should be there for Debra", 1, deliveryGroupsForDebra.Count);
			if (string.IsNullOrEmpty(otherDocumentEmailSubject) && !withOtherDocNullDataContext)
			{
				AssertEquals("Subject line after replacements - because data context is set, it will use the company name, branch, menu name and the datacontextwrapper.tostring() to get the subject line",
					GlbCompany.CurrentCompany.GC_Name.ToString() + " (" + GlbBranch.CurrentBranch.GB_BranchName.ToString() + ") - Shipment Document Command -", deliveryGroupsForDebra[0].SB_EmailSubjectLine);
			}
			else
			{
				AssertEquals("Mapped subject line", otherDocumentEmailSubject, deliveryGroupsForDebra[0].SB_EmailSubjectLine);
			}
		}

		void AssertReportSubjectLineMappings_WithOtherDocuments_DocPack(string otherDocumentEmailSubject, bool withOtherDocNullDataContext)
		{
			TestCaseHelper.ClearTable(StmDeliveryGroup.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			var helper = new IsDocPackTestHelper(this);
			helper.CreateTestData(false);
			helper.ConsolCommand.SU_IsDocPack = true;
			helper.ConsolCommand.SU_EmailSubjectLine = "Consol EmailSubject";
			helper.ShipmentCommand.SU_EmailSubjectLine = otherDocumentEmailSubject;
			if (withOtherDocNullDataContext)
			{
				helper.ShipmentCommand.SU_MenuDataContext = null;
			}
			Factory.Save();

			using (var printTask = CreateLoadedDummyDocumentPrintSet(helper.ConsolCommand, null))
			{
				AssertEquals("Count", 1, printTask.Count);

				var taskSettings = new PrintTaskSettings(printTask);
				taskSettings.Destination = DeliveryInstructionDestination.TakenFromContact;

				AssertEquals("Count", 1, taskSettings.DocPacksDeliveryInstructions.Count);

				var deliveryInstruction = taskSettings.DocPacksDeliveryInstructions[0];
				deliveryInstruction.Recipients.RemoveAndDeleteAll();
				var contact = deliveryInstruction.Recipients.AddNew();
				contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.Email = "dexter@morgan.com";

				printTask.Run(taskSettings);

				AssertEquals("Mapped subject line", "Consol EmailSubject", printTask.SubjectLineMappingsForPrintTaskForTesting[0].SubjectLine);
			}

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("One print job should be there", 1, printJobs.Length);

			var printJobsForDexter = printJobs.Where(job => job.SP_Destination == "dexter@morgan.com").ToList();
			AssertEquals("One print job should be there for Dexter", 1, printJobsForDexter.Count);
			AssertEquals("Email Subject correctly set", "ConsolDocument", printJobsForDexter[0].SP_EmailSubjectLine);

			var deliveryGroups = Factory.Load<StmDeliveryGroup>(new ZQuery());
			AssertEquals("One delivery group jobs should be there", 1, deliveryGroups.Length);

			var deliveryGroupsForDexter = deliveryGroups.Where(group => group.PK == printJobsForDexter[0].SP_SB_DeliveryGroup).ToList();
			AssertEquals("One delivery group should be there for Dexter", 1, deliveryGroupsForDexter.Count);
			AssertEquals("Email Subject correctly set", "Consol EmailSubject", deliveryGroupsForDexter[0].SB_EmailSubjectLine);
		}

		sealed class DummyBizoStorageFile : DummyBusinessObject, IStorageFile
		{
			public DummyBizoStorageFile(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public ZString DataType { get; set; }
			public ZDateTime DateAdded { get; set; }
			public ZString Description { get; set; }
			public ZString DocType { get; set; }
			public ZString DocSource { get; set; }

			public ZString DocSourceDescription { get; set; }
			public ZString FileName { get; set; }
			public ZString FileNameOnly { get; set; }
			public ZBlob ImageData { get; set; }
			public new ZBool IsDeleted { get; set; }
			public ZBool IsPublished { get; set; }
			public ZBool IsSystemGenerated { get; set; }
			public ZDateTime LastEdited { get; set; }
			public ZString LastEditedUser { get; set; }
			public void NotifyReadByUser() { }
			public ZGuid UniqueKey { get; set; }
			public Stream GetImageDataReader() => new CargoWise.IO.Shim.SubStreamableStream();
			public void SetImageDataStream(Stream stream) { }
		}

		public sealed class DummyBizoStorageDocs : DummyBusinessObject, IDeliverable, IStorageDocs, IeDoc
		{
			public DummyBizoStorageDocs(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public ZString DocumentDeliveredEventCode
			{
				get { throw new NotImplementedException(); }
			}

			public ZString DocumentPasswordInformationEventCode
			{
				get { throw new NotImplementedException(); }
			}

			public ZString FileExtension
			{
				get { throw new NotImplementedException(); }
			}

			public ZString Name
			{
				get { return ""; }
			}

			public ZPropertyInfo NameInfo
			{
				get { return GetZPropertyInfo(nameof(Name)); }
			}

			public void Save(DocDeliveryContact deliveryContact, DocDeliveryContact mostOfficialContact, System.IO.Stream fileContent)
			{
				throw new NotImplementedException();
			}

			public DeliveryMethods.DeliveryInfo GetDeliveryInfo(bool isDraft)
			{
				throw new NotImplementedException();
			}

			public ZString DeliveryMode
			{
				get { return ""; }
			}

			public ZPropertyInfo DeliveryModeInfo
			{
				get { return GetZPropertyInfo(nameof(DeliveryMode)); }
			}

			public ZString AllAvailableDeliveryModes
			{
				get { return nameof(PrintCopyType.ALL); }
			}

			public ZString DocumentTypeCode
			{
				get { return ""; }
			}

			public ZPropertyInfo DocumentTypeCodeInfo
			{
				get { return GetZPropertyInfo(nameof(DocumentTypeCode)); }
			}

			public ZString DocumentTypeDescription
			{
				get { return ""; }
			}

			public ZPropertyInfo DocumentTypeDescriptionInfo
			{
				get { return GetZPropertyInfo(nameof(DocumentTypeDescription)); }
			}

			public DocDeliveryPrintDetails PrinterDetails => throw new NotImplementedException();

			public ZGuid DeliveryGroupID
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ZBool IncludedInPrint { get; set; }

			public ZPropertyInfo IncludedInPrintInfo
			{
				get { return GetZPropertyInfo(nameof(IncludedInPrint)); }
			}

			public IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType()
			{
				return System.Array.Empty<string>();
			}

			public bool IncludedInPrint_ReadOnly { get; set; }

			public bool IsDeliveredByEmail { get; set; }

			public StmMenuItem MenuItem
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public ZBool CoverSheetRequired
			{
				get { throw new NotImplementedException(); }
			}

			public bool ContainsDataRows
			{
				get { throw new NotImplementedException(); }
			}

			public void DeleteTempFilesForTesting()
			{
				throw new NotImplementedException();
			}

			public int RunCountForTesting
			{
				get { throw new NotImplementedException(); }
			}

			public void Dispose()
			{
			}

			public string DocumentDeliveryMethod
			{
				get { throw new NotImplementedException(); }
			}

			public string DocumentName
			{
				get { throw new NotImplementedException(); }
			}

			public bool IncludeInPrint { get; set; }

			public ZGuid SourcePivotPK { get; set; }

			public ZGuid MenuTemplatePivotPK { get; }

			public ZGuid IdentifiablePK { get; }

			public bool CanIncludeInPrint { get; set; }

			public ZString DataType
			{
				get { throw new NotImplementedException(); }
			}

			public ZDateTime DateAdded
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ZString Description
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public ZString DocType
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}
			public ZString DocSource
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public ZString DocSourceDescription
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public ZString FileName
			{
				get { throw new NotImplementedException(); }
			}

			public ZString FileNameOnly
			{
				get { throw new NotImplementedException(); }
			}

			public ZBlob ImageData
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public new ZBool IsDeleted
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ZBool IsPublished
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public ZBool IsSystemGenerated
			{
				get { throw new NotImplementedException(); }
			}

			public ZDateTime LastEdited
			{
				get { throw new NotImplementedException(); }
			}

			public ZString LastEditedUser
			{
				get { throw new NotImplementedException(); }
			}

			public void NotifyReadByUser()
			{
				throw new NotImplementedException();
			}

			public bool SupportsDeliveryMethod(string deliveryMethod)
			{
				return false;
			}

			public IEnumerable<string> GetSupportedDeliveryMethods()
			{
				return System.Array.Empty<string>();
			}

			public ZGuid UniqueKey
			{
				get { throw new NotImplementedException(); }
			}

			public bool ShouldPrintByDefault { get; set; }

			public CodeDescriptionPairList DocType_List => throw new NotImplementedException();

			public BusinessObject ParentMain => throw new NotImplementedException();

			public ZString VisibleCompanyCode => throw new NotImplementedException();

			public ZString VisibleBranchCode => throw new NotImplementedException();

			public ZString VisibleDepartmentCode => throw new NotImplementedException();

			public ZBool IsCustomisableDocTypes => throw new NotImplementedException();

			public ZDecimal FileSizeInMB => throw new NotImplementedException();

			public ZString JobNumber { get; }

			public ZString NameForBinding { get; }

			public ZByte Index { get; set; }

			public ZString Identifier => "";

			public Stream GetImageDataReader()
			{
				throw new NotImplementedException();
			}

			public void SetImageDataStream(Stream stream)
			{
				throw new NotImplementedException();
			}

			public void SetValuesForTest(ZDateTime dateTime, ZString dataType)
			{
				throw new NotImplementedException();
			}

			public IDisposable OpenForEdit()
			{
				throw new NotImplementedException();
			}

			public string CreateReference()
			{
				throw new NotImplementedException();
			}
		}

		sealed class DummyDocumentPrintSet : DocumentPrintSet
		{
			public DummyDocumentPrintSet(DocumentCommand command, UserControlProviderList userFieldList)
				: base(command, userFieldList)
			{
			}

			public new void CheckAvailableDeliveryOptions(DeliveryInstructions instructions)
			{
				base.CheckAvailableDeliveryOptions(instructions);
			}

			public ReportSubjectLineMapping GenerateSubjectLineMappingForPrintTaskForTesting(DocumentPack docPack)
			{
				return base.GenerateSubjectLineMappingForPrintTask(docPack);
			}

			protected override ReportSubjectLineMapping GenerateSubjectLineMappingForPrintTask(DocumentPack docPack)
			{
				ReportSubjectLineMapping mapping = base.GenerateSubjectLineMappingForPrintTask(docPack);
				SubjectLineMappingsForPrintTaskForTesting.Add(mapping);
				return mapping;
			}

			public List<ReportSubjectLineMapping> SubjectLineMappingsForPrintTaskForTesting
			{
				get { return subjectLineMappingsForPrintTaskForTesting ?? (subjectLineMappingsForPrintTaskForTesting = new List<ReportSubjectLineMapping>()); }
			}
			List<ReportSubjectLineMapping> subjectLineMappingsForPrintTaskForTesting;
		}

		[DefaultField("Name")]
		sealed class DummyDataSource : DocumentWrapper
		{
			public DummyDataSource(string name = "Earl")
			{
				this.name = name;
			}

			public ZString Name
			{
				get { return name; }
			}
			readonly ZString name;
		}
	}
}
