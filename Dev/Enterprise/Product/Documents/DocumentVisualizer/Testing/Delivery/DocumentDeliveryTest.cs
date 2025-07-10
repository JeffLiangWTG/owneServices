using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[GuiTest]
	sealed class DocumentDeliveryTest : TestCaseWithFactory
	{
		public void TestDeliver()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();
			var logParent = Factory.New<DummyWithLogs>();

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					LogParent = logParent,
					PrintInstructions = new DummyPrintInstructions(),
					EDocsInstructions = new DummyEDocsInstructions()
				}
			};

			var delivery = new DocumentDelivery(supportable, deliveries);
			delivery.Deliver();

			AssertType(typeof(DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestDeliverMenuItemNotExtendFromStmMenuItem()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();
			var logParent = Factory.New<DummyWithLogs>();

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					LogParent = logParent,
					PrintInstructions = new DummyPrintInstructions(),
					EDocsInstructions = new DummyEDocsInstructions()
				}
			};

			var menuItem = new TestMenuItem();

			var delivery = new DocumentDelivery(supportable, deliveries, menuItem: menuItem);
			AssertNoExceptionThrown(() => delivery.Deliver());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplateWithImageDeliverWillNotThrowException()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();
			var logParent = Factory.New<DummyWithLogs>();

			var worksheet = TestFiles.CreateWorksheet(TestFiles.ImageTestTemplateFilePath);
			IStandardTemplate template = new StandardTemplate(worksheet);

			var document = template.CreateDocument(
				template.Name,
				"test",
				new MacroScope(Factory.New<DummyBusinessObject>().MakeDynamic()),
				new[] { new StandardLibrary() }.CreateContext(),
				null);

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = document,
				LogParent = logParent,
				PrintInstructions = new DummyPrintInstructions(),
				EDocsInstructions = new DummyEDocsInstructions()
			};

			var deliveries = new IDocumentDelivery[]
			{
				documentDelivery
			};

			var delivery = new DocumentDelivery(supportable, deliveries);
			delivery.Deliver();

			AssertNoExceptionThrown(() => documentDelivery.Document.ToXlsFile());
		}

		[ExpectNoExceptions]
		public void TestLicenceConsumptionOnDeliver()
		{
			var licenceConsumptionLogCreatorMock = new Mock<ILicenceConsumptionLogCreator>();

			licenceConsumptionLogCreatorMock.Setup(l => l.CreateLog(Env.Licence.FormBuilder));

			using (ObjectFactory.Substitute<ILicenceConsumptionLogCreator>(licenceConsumptionLogCreatorMock.Object))
			{
				var supportable = Factory.New<DummyDocumentSupportable>();
				var logParent = Factory.New<DummyWithLogs>();

				var deliveries = new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = new DummyDocument(),
						LogParent = logParent,
						PrintInstructions = new DummyPrintInstructions(),
						EDocsInstructions = new DummyEDocsInstructions()
					}
				};

				var delivery = new DocumentDelivery(supportable, deliveries);
				delivery.Deliver();
				delivery.Deliver();
			}
			licenceConsumptionLogCreatorMock.Verify(l => l.CreateLog(Env.Licence.FormBuilder), Times.Exactly(2));
		}

		public void TestDefaultPrinter()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();
			var logParent = Factory.New<DummyWithLogs>();
			var menuItem = Factory.New<IStmMenuItem>();

			var defaultPrinter = Factory.NewWithValidTestData<StmDefaultPrinter>();
			defaultPrinter.SDP_SU_Document = menuItem.PK;
			defaultPrinter.SDP_SubjectTableCode = GlbStaffSchema.Constants.Prefix;
			defaultPrinter.SDP_SubjectID = GlbStaff.CurrentUser.PK;

			menuItem.SU_ContactType = ContactType.All.Code;

			var otherPrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();

			Factory.Save();

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				var deliveries = new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = new DummyDocument(),
						LogParent = logParent,
						PrintInstructions = new DummyPrintInstructions(),
						EDocsInstructions = new DummyEDocsInstructions()
					}
				};

				var delivery = new DocumentDelivery(supportable, deliveries, menuItem: menuItem);
				var callCount = 0;
				mockPrintTaskUIProvider.Setup(x => x.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>()))
					.Returns(true)
					.Callback<PrintTask, DeliveryInstructions, ISecurityCheckpoint>((printTask, deliveryInstructions, securityCheckpoint) =>
					{
						AssertEquals("print task should be linked to menu item", menuItem.PK, printTask.DeliveryInstructionsDefaultPK);
						AssertEquals("delivery instructions should be linked to default printer", defaultPrinter.SDP_SQ_Printer, deliveryInstructions.PrinterDelivery.PrintQueuePK);
						callCount++;
						deliveryInstructions.PrinterDelivery.PrintQueuePK = otherPrintQueue.PK;
					});

				delivery.Deliver();
				AssertEquals("overrider called exactly once", 1, callCount);
				var updatedDefaultPrinter = Factory.Load<StmDefaultPrinter>(defaultPrinter.PK);
				AssertEquals("default printer should be updated to otherPrintQueue", otherPrintQueue.PK, updatedDefaultPrinter.SDP_SQ_Printer);
			}
		}

		#region Implementation

		class TestMenuItem : IStmMenuItem
		{
			public ZGuid PK => ZGuid.NewZGuid();

			public ZBlob SU_ActionDataUpdateBlob { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBlob SU_ActionMenusAndMethodsBlob { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_AddressCategory { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_AllowRawView { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_BusinessContext { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_ContactType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_DocumentDirection { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_DraftOption { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_EmailSubjectLine { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_FilterList { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZDecimal SU_FlexCelLineSpacing { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_GS_NKStaffCode { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_Hint { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_IncludeDocInArchive { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_IsClientSpecific { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_DeliveryRestrictionType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_DeliveryRestrictionMacro { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_DeliveryRestrictionDescription { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_IsDocPack { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_IsLocalDocument { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_IsModifiable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_IsPublished { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_IsSystemDefined { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_LicenceLevel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_MenuDataContext { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZShort SU_MenuIndex { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_MenuName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_Purpose { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

			public MultilingualString SU_MenuNameMultilingual => throw new System.NotImplementedException();

			public ZString SU_MenuPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_MenuShortcut { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_MenuType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_MustRunOnline { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_PreventAutoDelivery { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_SE_NKDocumentEvent { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_ShowDocToSendTab { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_SupportsVisualisation { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool SU_IsZippedDocPack { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZGuid SU_PrimaryDocPackItemId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_SignBy { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZString SU_EmailSenderOverride { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZBool AllowMultipleCopies { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public bool IsLocalDocument { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ZShort NumberOfCopies { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public CultureInfo RenderCulture { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

			public ZString DocumentId => throw new System.NotImplementedException();

			public ZString SU_DefaultAttachmentType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

			public ICodeDescriptionPairList AttachmentTypes => throw new System.NotImplementedException();

			public ICollection Documents => throw new System.NotImplementedException();

			public ICollection ChildMenus => throw new System.NotImplementedException();
		}

		#endregion
	}
}
