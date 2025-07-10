using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Business.Testing;
using Enterprise.DocumentEngine.DocBuilder.Testing;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentDeliveryManagerTest : StmDocumentDeliveryManagerTest
	{
		public void TestDeliveryQuotesSystemDocumentWithEmptySection()
		{
			ErrorReporter.Clear();
			var quote = Factory.New<IQuote>();
			var command = Factory.New<DocumentCommand>();
			var docBuilderTemplate = Factory.New<StmTemplateBase>();
			docBuilderTemplate.SO_DataContext = "Quotation";
			docBuilderTemplate.SO_Name = Core.Constants.SectionRepositoryTemplateNames.System;
			docBuilderTemplate.SO_Template = SectionRepositoryTestHelper.GetConfigurableTemplateBlob("No need for cellText here");

			var menu = Factory.New<StmMenuItemBase>();
			menu.SU_MenuName = "Menu";

			var pivot = menu.Documents.AddNew();
			pivot.SI_DocumentTitle = "Pivot";
			pivot.SI_SU = menu.PK;
			pivot.SI_SO = docBuilderTemplate.PK;

			command.SU_PrimaryDocPackItemId = pivot.PK;
			command.SU_BusinessContext = "Quotation";

			var builder = new RatingDocPackBuilder(command, quote as IDocumentSupportable, Callback);
			var dummyWrapper = new DocumentCommandTest.DocDummyBusinessObject.DummyWrapper();
			builder.AddPages(pivot, new DocumentWrapper[] { dummyWrapper });

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_IsProcessed = false;
			documentDelivery.SDL_SU = command.PK;
			documentDelivery.SDL_ParentId = ((BusinessObject)quote).PK;
			documentDelivery.SDL_ParentControllerIdOrTableCode = "Quotation";
			documentDelivery.SDL_RetryAttempts = 0;
			documentDelivery.SDL_Instructions = "{\"Language\":\"EN-GB\",\"NumberOfCopies\":\"1\",\"Recipients\":[{\"DeliveryMethod\":\"EML\",\"AttachmentType\":\"PDF\",\"DeliveryAddress\":\"b@b.com\"}],\"DocumentsToBeDelivered\":[{\"Copies\":\"1\",\"Identifier\":\"" +  builder.DocPack[0].Identifier +
												"\"}],\"EDocsToBeDelivered\":[]}";
			documentDelivery.SDL_GS = GlbStaff.CurrentUser.PK;
			documentDelivery.SDL_GB = GlbBranch.CurrentBranch.PK;
			documentDelivery.SDL_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			var documentDeliveryManager = new DocumentDeliveryManager();

			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				AssertNoExceptionThrown(() => documentDeliveryManager.ProcessDocumentDelivery(documentDelivery));
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestTestDeliveryQuotesDocumentWithMacroDocumentTitle()
		{
			ErrorReporter.Clear();
			var quote = Factory.New<IQuote>();
			var documentMenuCustomisation = DocumentMenuCustomisation.New(quote as IDocumentSupportable, null, Factory);

			var stmMenuItemBase = (from StmMenuItemBase a in documentMenuCustomisation.AvailableChildMenus
				where a.SU_BusinessContext == "Quotation" && a.SU_MenuName == "Quotation Pack"
				select a).FirstOrDefault();

			var documentCommand = documentMenuCustomisation.Menus.AddNew();
			var stmMenuMenuPivotBase = documentCommand.ChildMenus.AddNew();
			stmMenuMenuPivotBase.SF_SU_Inward = documentCommand.PK;
			stmMenuMenuPivotBase.SF_SU_Outward = stmMenuItemBase.PK;
			documentCommand.SU_IsDocPack = true;
			documentCommand.SU_PrimaryDocPackItemId = stmMenuMenuPivotBase.PK;
			stmMenuItemBase.Documents[0].SI_DocumentTitle = "<JobNumber>";
			var builder = new RatingDocPackBuilder(documentCommand, quote as IDocumentSupportable, Callback);
			var dummyWrapper = new DocumentCommandTest.DocDummyBusinessObject.DummyWrapper();
			builder.AddPages(stmMenuItemBase.Documents[0], new DocumentWrapper[] { dummyWrapper });

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_IsProcessed = false;
			documentDelivery.SDL_SU = documentCommand.PK;
			documentDelivery.SDL_ParentId = ((BusinessObject)quote).PK;
			documentDelivery.SDL_ParentControllerIdOrTableCode = "Quotation";
			documentDelivery.SDL_RetryAttempts = 0;
			documentDelivery.SDL_Instructions = "{\"Language\":\"EN-GB\",\"NumberOfCopies\":\"1\",\"Recipients\":[{\"DeliveryMethod\":\"EML\",\"AttachmentType\":\"PDF\",\"DeliveryAddress\":\"b@b.com\"}],\"DocumentsToBeDelivered\":[{\"Copies\":\"1\",\"Identifier\":\"" + builder.DocPack[0].Identifier +
			"\"}],\"EDocsToBeDelivered\":[]}";
			documentDelivery.SDL_GS = GlbStaff.CurrentUser.PK;
			documentDelivery.SDL_GB = GlbBranch.CurrentBranch.PK;
			documentDelivery.SDL_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var documentDeliveryManager = new DocumentDeliveryManager();

			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				AssertNoExceptionThrown(() => documentDeliveryManager.ProcessDocumentDelivery(documentDelivery));
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestDeliveryDocumentWithEmptyTemplateName()
		{
			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals("Pre-condition: printJobs.Count", 0, printJobs.Count);

			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "DEFRA";

			var forwardingDocumentSupporter = shipment as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, Factory);
			var documentCommand = customization.Menus.AddNew();
			documentCommand.SU_MenuName = "Test Primary Doc" + ZGuid.NewZGuid();
			documentCommand.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_BusinessContext = "Shipment";
			documentCommand.SU_MenuIndex = 0;
			documentCommand.SU_FilterList = "\"<JS_TransportMode>\" == \"AIR\"";

			var template = customization.AvailableTemplates.Cast<StmTemplateBase>().FirstOrDefault(a => a.SO_DataContext == "Shipment");

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "";

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_IsProcessed = false;
			documentDelivery.SDL_SU = documentCommand.PK;
			documentDelivery.SDL_ParentId = shipment.PK;
			documentDelivery.SDL_ParentControllerIdOrTableCode = "JobShipment";
			documentDelivery.SDL_RetryAttempts = 0;
			documentDelivery.SDL_GS = staff.PK;
			documentDelivery.SDL_GB = branch.PK;
			documentDelivery.SDL_GE = department.PK;

			var docPack = new DocumentPack(documentCommand, forwardingDocumentSupporter, null, null, false);
			var deliveryInstructions = new DeliveryInstructions(docPack);
			deliveryInstructions.Recipients.RemoveAll();
			var contact = deliveryInstructions.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.DeliveryAddress = "test@test.com";

			var contextInfomation = new SerializableDeliveryInstructions();
			contextInfomation.SetDeliveryInstructionsContextInformation(deliveryInstructions);
			documentDelivery.SDL_Instructions = JsonConvert.SerializeObject(contextInfomation, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });

			Factory.Save();

			var documentDeliveryManager = new DocumentDeliveryManager();

			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				AssertNoExceptionThrown(() => documentDeliveryManager.ProcessDocumentDelivery(documentDelivery));
			}
			printJobs.Load();
			AssertEquals("printJobs.Count", 1, printJobs.Count);
			AssertEdocParentBO(shipment as BusinessObject);
		}

		void Callback(RatingDocPackBuilder builder)
		{
		}

		public override void TestProcessDocumentDelivery()
		{
			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (stmDocumentDelivery, _) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			stmDocumentDelivery.SDL_Instructions = @"
{
	""DocumentsToBeDelivered"": [
		{
			""Copies"": ""1"",
			""Identifier"": ""4O6phfxbZ3rBBhJAqkq2ag==""
		}
	],
	""EDocsToBeDelivered"": []
}
";
			var instructions1 = SerializableDeliveryInstructions.DeserializeDeliveryInstructions(stmDocumentDelivery);
			AssertEquals("DocumentsToBeDelivered True", true, !instructions1.DocumentsToBeDelivered.IsNullOrEmpty());
			AssertEquals("EDocsToBeDelivered False", false, !instructions1.EDocsToBeDelivered.IsNullOrEmpty());

			stmDocumentDelivery.SDL_Instructions = @"
{
	""EDocsToBeDelivered"": [
		{
			""Copies"": ""1"",
			""Identifier"": ""4O6phfxbZ3rBBhJAqkq2ag==""
		}
	],
	""EDocsToBeDelivered"": []
}
";
			var instructions2 = SerializableDeliveryInstructions.DeserializeDeliveryInstructions(stmDocumentDelivery);
			AssertEquals("DocumentsToBeDelivered False", false, !instructions2.DocumentsToBeDelivered.IsNullOrEmpty());
			AssertEquals("EDocsToBeDelivered True", true, !instructions2.EDocsToBeDelivered.IsNullOrEmpty());
		}

		public void TestProcessDocumentDelivery_DocumentDeliveryCore_Consol()
		{
			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals("Pre-condition: printJobs.Count", 0, printJobs.Count);

			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (documentDelivery, consol) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Consol(Factory, staff, branch, department);
			var documentDeliveryManager = new DocumentDeliveryManager();

			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				AssertNoExceptionThrown(() => documentDeliveryManager.ProcessDocumentDelivery(documentDelivery));
			}
			printJobs.Load();
			AssertEquals("printJobs.Count", 1, printJobs.Count);
			AssertEdocParentBO(consol as BusinessObject);
			AssertDDVEventExists(consol as BusinessObject);
		}

		public void TestProcessDocumentDelivery_DocumentDeliveryCore_Shipment()
		{
			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals("Pre-condition: printJobs.Count", 0, printJobs.Count);

			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (documentDelivery, shipment) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(Factory, staff, branch, department);
			var documentDeliveryManager = new DocumentDeliveryManager();

			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				AssertNoExceptionThrown(() => documentDeliveryManager.ProcessDocumentDelivery(documentDelivery));
			}
			printJobs.Load();
			AssertEquals("printJobs.Count", 1, printJobs.Count);
			AssertEdocParentBO(shipment as BusinessObject);
			AssertDDVEventExists(shipment as BusinessObject);
		}

		void AssertEdocParentBO(BusinessObject expectedBO)
		{
			var query = new ZQuery(StmPrintJobSchema.SP_ParentTableName, expectedBO.TableName);
			query.AddToFilter(StmPrintJobSchema.SP_ParentGuid, expectedBO.PK);
			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull($"Excepted parent type: {expectedBO.GetType()}", printJob);
		}

		void AssertDDVEventExists(BusinessObject expectedBO)
		{
			var query = new ZQuery(StmALogSchema.SL_Table, expectedBO.TableName);
			query.AddToFilter(StmALogSchema.SL_Parent, expectedBO.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DocumentDeliveredCode);
			var log = Factory.LoadTop1<StmALog>(query);
			AssertNotNull($"{expectedBO.GetType()} should have DDV event ", log);
		}

		public void TestGetDocumentSupportableFallback()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			Factory.Save();

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_ParentControllerIdOrTableCode = JobDeclarationSchema.Constants.Prefix;
			documentDelivery.SDL_ParentId = declaration.PK;
			var manager = new DocumentDeliveryManagerForTest();
			AssertEquals(declaration.PK, manager.GetDocumentSupportableAsBusinessObject(documentDelivery).PK);

			documentDelivery.SDL_ParentControllerIdOrTableCode = JobDeclarationSchema.Constants.TableName;
			documentDelivery.SDL_ParentId = declaration.PK;
			AssertEquals(declaration.PK, manager.GetDocumentSupportableAsBusinessObject(documentDelivery).PK);
		}

		public void TestGetDocumentSupportableWhenFormBizoDoesNotEqualsToControllerBizo()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_ParentControllerIdOrTableCode = "JobDeclarationPluggedIntoShipment";
			documentDelivery.SDL_ParentId = declaration.PK;
			var manager = new DocumentDeliveryManagerForTest();
			AssertNotNull(manager.GetDocumentSupportable_Exposed(documentDelivery));
		}

		public void TestGetDocumentSupportableFromPlugInController()
		{
			var manager = new DocumentDeliveryManagerForTest();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_ParentControllerIdOrTableCode = "JobDeclarationPluggedIntoShipment";
			documentDelivery.SDL_ParentId = shipment.PK;
			AssertNotNull(manager.GetDocumentSupportable_Exposed(documentDelivery));
		}

		public void TestGetCorrectDocumentSupportable()
		{
			var manager = new DocumentDeliveryManagerForTest();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_ParentControllerIdOrTableCode = "JobDeclarationPluggedIntoShipment";
			documentDelivery.SDL_ParentId = shipment.PK;
			AssertEquals(manager.GetDocumentSupportableAsBusinessObject(documentDelivery).PK, shipment.PK);

			documentDelivery.SDL_ParentControllerIdOrTableCode = "JobDeclaration";
			documentDelivery.SDL_ParentId = declaration.PK;
			AssertEquals(manager.GetDocumentSupportableAsBusinessObject(documentDelivery).PK, declaration.PK);
		}

		public void TestDocumentDeliveryNotInclude()
		{
			ErrorReporter.Clear();

			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (documentDelivery, consol) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_Consol(Factory, staff, branch, department);
			var documentDeliveryManager = new DocumentDeliveryManager();

			documentDelivery.SDL_Instructions = "{\"Language\":\"EN-GB\",\"NumberOfCopies\":\"1\",\"Recipients\":[{\"DeliveryMethod\":\"EML\",\"AttachmentType\":\"PDF\",\"DeliveryAddress\":\"b@b.com\"}],\"DocumentsToBeDelivered\":[{\"Copies\":\"1\",\"Identifier\":\"4O6phfxbZ3rBBhJAqkq2ag==\"}],\"EDocsToBeDelivered\":[]}";

			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				AssertNoExceptionThrown(() => documentDeliveryManager.ProcessDocumentDelivery(documentDelivery));
			}

			AssertEquals("DeliverablesToBePrinted Not Matched To StmDocumentDelivery", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestBackGroundDeliveryRelatedCorrectBusinessContextWhenCreatePrintJob()
		{
			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals(0, printJobs.Count);

			var (staff, branch, department) = DocumentDeliveryManagerTestAssistant.InitialEnv(Factory);
			var (documentDelivery, dtbBooking) = DocumentDeliveryManagerTestAssistant.CreateNewDocumentDeliveryWithDocumentSupportable_DtbBooking(Factory, staff, branch, department);
			var documentDeliveryManager = new DocumentDeliveryManager();

			documentDeliveryManager.ProcessDocumentDelivery(documentDelivery);
			printJobs.Load();

			AssertEquals(1, printJobs.Count);
			AssertEquals("DtbBooking", printJobs[0].SP_ParentTableName);
			AssertEquals(dtbBooking.PK, printJobs[0].SP_ParentGuid);
			AssertEquals("DTB", printJobs[0].SP_RelatedBusinessContext);
			ErrorReporter.Clear();
		}

		class DocumentDeliveryManagerForTest : DocumentDeliveryManager
		{
			public IDocumentSupportable GetDocumentSupportable_Exposed(StmDocumentDelivery stmDocumentDelivery)
			{
				return GetDocumentSupportable(stmDocumentDelivery);
			}

			public BusinessObject GetDocumentSupportableAsBusinessObject(StmDocumentDelivery stmDocumentDelivery)
			{
				return (BusinessObject)GetDocumentSupportable(stmDocumentDelivery);
			}
		}
	}
}
