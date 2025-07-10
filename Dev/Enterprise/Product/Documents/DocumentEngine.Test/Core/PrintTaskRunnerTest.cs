using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class PrintTaskRunnerTest : TestCaseWithFactory
	{
		public void TestBusinessEntity()
		{
			PrintTaskRunner ptr = new PrintTaskRunner();
			OrgHeader organisation = Factory.New<OrgHeader>();

			DocumentZQuery filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Communication Report");
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			ptr.RunPrintTaskIncludingChildMenus(menuItem, organisation);

			AssertNotNull("DocumentPack should not equal null", ptr.LastCreatedDocPack);
			AssertNotNull("PrintTask should not equal null", ptr.LastCreatedPrintTask);
			AssertEquals("PrintTask should contain 1 element", 1, ptr.LastCreatedPrintTask.Count);
		}

		public void TestRunPrintTask()
		{
			var printTaskRunner = new PrintTaskRunner();
			var organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_Email = "jim.flin@lim.com";

			var filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Communication Report");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			printTaskRunner.RunPrintTask(menuItem, organisation);

			AssertNotNull("DocumentPack must be created", printTaskRunner.LastCreatedDocPack);
			AssertNotNull("PrintTask must be created", printTaskRunner.LastCreatedPrintTask);
			AssertEquals("PrintTask must contain correct document menu item to save printer to.", menuItem.PK, printTaskRunner.LastCreatedPrintTask.DeliveryInstructionsDefaultPK);
			AssertEquals("PrintTask must contain 1 element", 1, printTaskRunner.LastCreatedPrintTask.Count);
			AssertEquals("PrintTask must correct DocPack", printTaskRunner.LastCreatedDocPack, printTaskRunner.LastCreatedPrintTask[0]);
			AssertEquals(organisation, printTaskRunner.LastCreatedDocPack.DocumentSupporter.BusinessObject);
			AssertEquals(menuItem.PK, printTaskRunner.LastCreatedDocPack.StmMenuCommand.PK);

			foreach (DocDeliveryContact recipient in printTaskRunner.DeliveryInstructionsForTest.Recipients)
			{
				AssertEquals(Core.Constants.ContactNotifyModes.Print, recipient.DeliveryMethod);
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunPrintTaskWithChildMenusAndEDocs_ShouldAddAllToPrintTask()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = Factory.New<DummyDocManagerTestBizO>();
			dummy.SetupDocManagerObjects();
			((DummyDocManagerTestBizODocumentSupporter)dummy.DocumentSupporter).BusinessContextOverride = BusinessContext.Shipment;
			var template = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");

			var command = helper.CreateDocCommand("Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			command.Parent = dummy;
			command.SU_IsDocPack = true;
			helper.CreateMenuTemplatePivot("Pub System Shipment Document", template, command);

			var childCommand = helper.CreateDocCommand("Child Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			childCommand.Parent = dummy;
			helper.CreateMenuTemplatePivot("Child Pub System Shipment Document", helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 2"), childCommand);
			helper.CreateMenuMenuPivot(command, childCommand);

			var consumer = MockEDocsProvider.New(Factory, BusinessContext.Shipment);
			var provider1Placeholder = consumer.GetEDocsProviderSupporter().CreateProviderPlaceholder<DocumentCommand>(command);
			var mainEDocPivot = command.EDocs.AddNew();
			mainEDocPivot.SX_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "CCC")).PK;

			Factory.Save();

			var runner = new PrintTaskRunner();
			runner.RunPrintTaskIncludingChildMenus(command, dummy);

			var task = runner.LastCreatedPrintTask;
			var packs = task.GetDocumentPacks().ToArray();
			AssertEquals("Should be one pack in print task", 1, packs.Length);

			var pack = packs[0];
			var includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludeInPrint);
			var documentNames = pack.Cast<IDeliverable>().Select(d => d.Name).ToArray();
			var notPrintByDefault = pack.Cast<IDeliverable>().Count(doc => !doc.ShouldPrintByDefault);

			AssertEquals("Should be 5 documents in pack", 5, pack.Count);
			AssertEquals("All eDocs IncludeInPrint should be reset to true", 5, includeCount);
			AssertEquals("Should be 3 documents which is not print by default", 3, notPrintByDefault);

			AssertCollectionContains("A Test Document AAA", documentNames);
			AssertCollectionContains("A Test Document BBB", documentNames);
			AssertCollectionContains("A Test Document CCC", documentNames);
			AssertCollectionContains("Pub System Shipment Document", documentNames);
			AssertCollectionContains("Child Pub System Shipment Document", documentNames);
		}
	}
}
