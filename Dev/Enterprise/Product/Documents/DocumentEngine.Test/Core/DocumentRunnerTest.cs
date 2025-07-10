using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentRunnerTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestBackgroundDeliveryInMultiThread()
		{
			var stmDocumentDelivery1 = Factory.New<StmDocumentDelivery>();
			var stmDocumentDelivery2 = Factory.New<StmDocumentDelivery>();

			var documentDeliveryHashInThread1 = 0;
			var documentDeliveryHashInThread2 = 0;

			var thread1 = new Thread(() =>
			{
				using (DocumentRunner.BackgroundDelivery(stmDocumentDelivery1))
				{
					Thread.Sleep(100);
					documentDeliveryHashInThread1 = DocumentRunner.DocumentDelivery.GetHashCode();
				}
			});
			var thread2 = new Thread(() =>
			{
				using (DocumentRunner.BackgroundDelivery(stmDocumentDelivery2))
				{
					documentDeliveryHashInThread2 = DocumentRunner.DocumentDelivery.GetHashCode();
				}
			});

			thread1.Start();
			thread2.Start();

			thread2.Join();
			thread1.Join();

			AssertEquals(stmDocumentDelivery1.GetHashCode(), documentDeliveryHashInThread1);
			AssertEquals(stmDocumentDelivery2.GetHashCode(), documentDeliveryHashInThread2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunDocumentWhenChildDocumentSupportableIsNull()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var command = helper.CreateDocCommand("Parent");
			command.Parent = dummy;

			var template = helper.CreateTemplate(nameof(Core.Constants.DataContext.WhsOrder), "Template");
			var childCommand = helper.CreateDocCommand("Child");
			childCommand.SU_BusinessContext = nameof(BusinessContext.WhsOrder);
			helper.CreateMenuMenuPivot(command, childCommand);
			helper.CreateMenuTemplatePivot("Child", template, childCommand);

			Factory.Save();

			var runner = new DocumentRunner(null, null, null, null, null);
			runner.Run(command);

			AssertEquals("Cannot produce this Document because the required data is not present", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunDocumentWhenChildDocumentSupportableIsOverrideWithEmptyArray()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var dummy = Factory.NewWithValidTestData<DocDummyBusinessObjectWithOverrideSupporter>();
			var command = helper.CreateDocCommand("Parent");
			command.Parent = dummy;

			var template = helper.CreateTemplate(nameof(Core.Constants.DataContext.WhsOrder), "Template");
			var childCommand = helper.CreateDocCommand("Child");
			childCommand.SU_BusinessContext = nameof(BusinessContext.WhsOrder);
			helper.CreateMenuMenuPivot(command, childCommand);
			helper.CreateMenuTemplatePivot("Child", template, childCommand);

			Factory.Save();

			var runner = new DocumentRunner(null, null, null, null, null);
			runner.Run(command);

			AssertNull("No need to show message as supporter is override with GetChildCollection.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDocumentSecurity()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "111";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "222";
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_LoginName = "333";
			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_LoginName = "444";
			ModuleIdentifier moduleIDForSecurity = ModuleIDs.JobShipment;
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_MenuPath, "Legacy Documents/Arrival"));

			GlbSecurity parentSecurity1 = Factory.New<GlbSecurity>();
			parentSecurity1.GU_SecurityRight = "DocJobShipmentDocuments";
			parentSecurity1.GU_GS = staff1.PK;
			parentSecurity1.GU_SecurityItemIsAllowed = true;

			GlbSecurity parentSecurity2 = Factory.New<GlbSecurity>();
			parentSecurity2.GU_SecurityRight = "DocJobShipmentDocuments";
			parentSecurity2.GU_GS = staff2.PK;
			parentSecurity2.GU_SecurityItemIsAllowed = true;
			GlbSecurity childSecurity2 = Factory.New<GlbSecurity>();
			childSecurity2.GU_SecurityRight = "DocJobShipment";
			childSecurity2.GU_ItemGUID = command.PK;
			childSecurity2.GU_GS = staff2.PK;
			childSecurity2.GU_SecurityItemIsAllowed = false;

			GlbSecurity parentSecurity3 = Factory.New<GlbSecurity>();
			parentSecurity3.GU_SecurityRight = "DocJobShipmentDocuments";
			parentSecurity3.GU_GS = staff3.PK;
			parentSecurity3.GU_SecurityItemIsAllowed = false;
			GlbSecurity childSecurity3 = Factory.New<GlbSecurity>();
			childSecurity3.GU_SecurityRight = "DocJobShipment";
			childSecurity3.GU_ItemGUID = command.PK;
			childSecurity3.GU_GS = staff3.PK;
			childSecurity3.GU_SecurityItemIsAllowed = true;

			GlbSecurity parentSecurity4 = Factory.New<GlbSecurity>();
			parentSecurity4.GU_SecurityRight = "DocJobShipmentDocuments";
			parentSecurity4.GU_GS = staff4.PK;
			parentSecurity4.GU_SecurityItemIsAllowed = false;

			Factory.Save();

			DocumentRunner runner = new DocumentRunner(null, moduleIDForSecurity, null, null, null);
			SecurityCheckpoint dummy = null;

			var context = new TemporaryUserContext();
			context.StaffLoginName = staff1.GS_LoginName;
			using (context.Set())
			{
				AssertEquals(true, runner.CheckIfPrintingIsAllowed(command, out dummy));
			}

			context = new TemporaryUserContext();
			context.StaffLoginName = staff2.GS_LoginName;
			using (context.Set())
			{
				AssertEquals(false, runner.CheckIfPrintingIsAllowed(command, out dummy));
			}

			context = new TemporaryUserContext();
			context.StaffLoginName = staff3.GS_LoginName;
			using (context.Set())
			{
				AssertEquals(true, runner.CheckIfPrintingIsAllowed(command, out dummy));
			}

			context = new TemporaryUserContext();
			context.StaffLoginName = staff4.GS_LoginName;
			using (context.Set())
			{
				AssertEquals(false, runner.CheckIfPrintingIsAllowed(command, out dummy));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRun_DataContextExceptionPresentedToUser()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_DraftOption = DraftOptionsList.Codes.Draft;

			var excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightMacroWithinRowContainingMergedRowsInOtherColumn.xls", TestFilesSubFolder.DocumentTestFiles);
			var template = Factory.New<StmTemplateBase>();
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = "WrongValue";
			template.SO_Name = "Test Template";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var dr = new DocumentRunner();
			dr.Run(documentCommand);

			var expectedMessage = "The DataContext parameter in the #config of the template has been entered incorrectly. Please check the value and then try generating the document again.";
			var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals(expectedMessage, actualMessage);
		}

		public void TestRun_FetchStrategyInitialised()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var runner = new DocumentRunner(null, null, new DocumentEventsForMenuForTesting(), null, null);

			var fetchStrategy = (DummyFetchStrategy)dummy.FetchStrategy;
			Assert("Expecting Document Fetch Strategy not yet set up", !fetchStrategy.FetchStrategyInitialised);
			runner.Run(documentCommand);
			Assert("Expecting Document Fetch Strategy to be set up", fetchStrategy.FetchStrategyInitialised);
		}

		public void TestRunWithException_ShouldIncludeMenuDetailsInException()
		{
			var parent = new Mock<IDocumentSupportable>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Mai Frist Document";
			command.SU_IsSystemDefined = true;
			command.SU_MenuPath = "MaiStuff";
			command.Parent = parent.Object;

			try
			{
				new DocumentRunner().Run(command);

				Fail("Should have thrown an exception");
			}
			catch (DocumentEngineException ex)
			{
				var expectedMessage = "Could not deliver the following document menu item:\r\n\tMenu Path: MaiStuff\r\n\tMenu Name: Mai Frist Document\r\n\tSystem Defined: Y\r\n\r\nThe following error occurred:\r\nObject reference not set to an instance of an object.\r\nSee inner exception for details.";
				AssertEquals(expectedMessage, ex.Message);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsDraftIsDeliveredWhenDocumentIsPrinted()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_DraftOption = DraftOptionsList.Codes.Draft;

			var excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightMacroWithinRowContainingMergedRowsInOtherColumn.xls", TestFilesSubFolder.DocumentTestFiles);

			var template = Factory.New<StmTemplateBase>();
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = "UnitTest";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			Assert("IsDraft is passed from event arguments.", ReturnIsDraftValueWhenDocumentIsPrinted(documentCommand));

			documentCommand.SU_DraftOption = DraftOptionsList.Codes.Final;
			Assert("IsDraft is passed from event arguments.", !ReturnIsDraftValueWhenDocumentIsPrinted(documentCommand));
		}

		public void TestISupportCustomizedDocumentPrintSet()
		{
			var documentEventsForMenuForTesting = new DocumentEventsForMenuForTesting();
			var runner = new DocumentRunner(null, null, documentEventsForMenuForTesting, null, null);

			var dummy = Factory.New<DummyBODocSupportableWithCustomizedDocumentPrintSet>();
			Assert(!((DummyBODocSupportableDocumentSupporterWithCustomizedDocumentPrintSet)dummy.DocumentSupporter).documentPrintSetWithStreamingWasCalled);
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;

			runner.Run(documentCommand);
			Assert("Should use the document supporter implementing interface ISupportCustomizedDocumentPrintSet", dummy.DocumentSupporter is ISupportCustomizedDocumentPrintSet);
			Assert(((DummyBODocSupportableDocumentSupporterWithCustomizedDocumentPrintSet)dummy.DocumentSupporter).documentPrintSetWithStreamingWasCalled);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowReasonForNotPrinting()
		{
			var pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Test);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "NULL";
			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightMacroWithinRowContainingMergedRowsInOtherColumn.xls", TestFilesSubFolder.DocumentTestFiles);
			var template = Factory.New<StmTemplateBase>();
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = "BusinessObject";
			template.SO_Name = "Test Template";

			var mockDummyBODocSupportable = new Mock<DummyBODocSupportable>(Factory, (new RowFactory()).New(BusinessObjectFactory.GetViewNameFromType(typeof(DummyBODocSupportable))));
			var mockDocumentSupporter = new Mock<DummyBODocSupportableDocumentSupporter>(mockDummyBODocSupportable.Object);
			mockDocumentSupporter.Setup(x => x.BusinessContext).Returns(BusinessContext.Test);

			mockDummyBODocSupportable.Setup(x => x.DocumentSupporter).Returns(mockDocumentSupporter.Object);
			mockDummyBODocSupportable.Setup(x => x.DocManagerInfo)
				.Returns(new DocManagerInfo(mockDummyBODocSupportable.Object, Enterprise.Core.Constants.DocManagerCodes.Shipment));

			mockDummyBODocSupportable.Object.Z0_Code = "Tst";
			mockDummyBODocSupportable.Object.Z0_Description = "Desc";

			var command = DocumentCommand.GetDocumentCommand(Factory, mockDummyBODocSupportable.Object, "NULL");
			command.Parent = mockDummyBODocSupportable.Object;

			mockDocumentSupporter.Setup(x => x.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.BusinessObject, It.Is<DocumentCommand>(p => p.SU_MenuName == "NULL"))).Returns(true);
			mockDocumentSupporter
				.Setup(x => x.GetBODocDataProvidersNotFoundMessage(
					new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.BusinessObject)), It.Is<DocumentCommand>(p => p.SU_MenuName == "NULL"))).Returns("NOT FOUND");

			var document = command.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var runner = new DocumentRunner();
			UnitTestUserNotification.Instance.ClearMessages();
			runner.Run(command);
			AssertEquals(0, runner.GetDocumentPrintSetForTesting(command).GetDocumentPacks().Count());
			var expectedMessage = @"Cannot produce this Document because the data required to do so is not present
Details:
NOT FOUND";
			var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("Should still show reason for printset without document pack", expectedMessage, actualMessage);
		}

		public void TestShowReasonsForNotPrinting_WhenThereIsNullValueInReasonList()
		{
			var runner = new DocumentRunner();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = null;

			var documentPrintSet = runner.GetDocumentPrintSetForTesting(documentCommand);
			documentPrintSet.ReasonsForEmptyPacks.Add(null);

			AssertNoExceptionThrown("No NullReferenceException is thrown out", () => runner.ShowReasonsForNotPrinting(documentPrintSet));
		}

		public void TestRun_ExternalStorageExceptionShouldBeHandled()
		{
			var isReportExceptionForDeveloperCalled = false;
			var externalStorageExceptionMock = new Mock<ExternalStorageException>("", "S3", null);
			externalStorageExceptionMock.SetupGet(e => e.UnableToAccessStorageFriendlyMessage).Returns("Failed to access external storage");
			externalStorageExceptionMock.Setup(e => e.ReportExceptionForDeveloper()).Callback(() => isReportExceptionForDeveloperCalled = true);

			var documentCommand = Factory.New<DocumentCommand>();
			var runner = new DocumentRunner(null, null, new DocumentEventsForMenuForTesting(), null, null)
			{
				ThrowExceptionInGetDocumentPrintSetForTesting = () => throw externalStorageExceptionMock.Object
			};

			AssertNoExceptionThrown("No Exception should be thrown out", () => runner.Run(documentCommand));
			AssertEquals("Failed to access external storage", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("ReportExceptionForDeveloper is called", true, isReportExceptionForDeveloperCalled);
		}

		[GuiTest]
		public void TestStaffWithoutDocumentsSecurityRightCanRunOwnPrivateDocuments()
		{
			UnitTestUserNotification.Instance.ClearMessages();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Test User";

			var documentSecurity = Factory.New<GlbSecurity>();
			documentSecurity.GU_SecurityRight = "DocDtbBookingDocuments";
			documentSecurity.GU_GS = staff.PK;
			documentSecurity.GU_SecurityItemIsAllowed = false;

			var dummy = Factory.New<DummyDocumentSupportable>();
			var command = Factory.New<DocumentCommand>();
			command.Parent = dummy;
			command.SU_MenuName = "Custom Document";
			command.SU_IsPublished = true;

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "UnitTest";
			var document = command.Documents.AddNew();
			document.SI_SU = command.PK;
			document.SI_SO = template.PK;

			Factory.Save();

			var runner = new DocumentRunner(null, ModuleIDs.DtbBooking, null, null, null);

			using (var userContext = Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				runner.Run(command);

				var expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Transport Booking -> Bookings -> Documents -> Custom Document";
				AssertEquals("Precondition: A user without documents security rights cannot run/modify a published document", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				command.SU_IsPublished = false;
				Factory.Save();
				runner.Run(command);

				AssertNull("A user without douments security rights should be able to run/modify their own document that isn't published yet", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, runner.CheckIfPrintingIsAllowed(command, out var securityCheckpoint));
			}
		}

		bool ReturnIsDraftValueWhenDocumentIsPrinted(DocumentCommand documentCommand)
		{
			var documentEventsForMenuForTesting = new DocumentEventsForMenuForTesting();
			bool? isDraft = null;
			documentEventsForMenuForTesting.DocumentPrinted += (object sender, DocumentPrintedEventArgs e) => { isDraft = e.IsDraft; };
			var runner = new DocumentRunner(null, null, documentEventsForMenuForTesting, null, null);
			runner.Run(documentCommand);
			return isDraft.Value;
		}
	}
}
