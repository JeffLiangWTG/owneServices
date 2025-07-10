using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentPackLoaderTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommands_ShouldLoadDescendentTree()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Template");
			var command = helper.CreateDocCommand("Parent");
			command.SU_IsDocPack = true;
			command.Parent = dummy;
			helper.CreateMenuTemplatePivot("Parent", template, command);

			var childCommand1 = helper.CreateDocCommand("Child1");
			helper.CreateMenuMenuPivot(command, childCommand1);
			helper.CreateMenuTemplatePivot("Child1", template, childCommand1);

			var childCommand2 = helper.CreateDocCommand("Child2");
			helper.CreateMenuMenuPivot(command, childCommand2);
			helper.CreateMenuTemplatePivot("Child2", template, childCommand2);

			var grandChildCommand = helper.CreateDocCommand("Grandchild");
			helper.CreateMenuMenuPivot(childCommand1, grandChildCommand);
			helper.CreateMenuTemplatePivot("Grandchild", template, grandChildCommand);

			Factory.Save();

			using (var printTask = new PrintTask())
			{
				var parentLoader = new PrintTaskDocumentPackLoader(printTask, command, null);
				parentLoader.LoadAll();

				var pack = printTask.GetDocumentPacks().First();
				AssertEquals("Should be four documents in pack", 4, pack.Count);
				AssertEquals("Parent", pack[0].Name);
				AssertEquals("Child1", pack[1].Name);
				AssertEquals("Grandchild", pack[2].Name);
				AssertEquals("Child2", pack[3].Name);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommands_ChildrenHaveCorrectParent()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummyParent = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var template = helper.CreateTemplate(nameof(Core.Constants.DataContext.Shipment), "Template");
			var command = helper.CreateDocCommand("Parent");
			command.SU_IsDocPack = true;
			command.Parent = dummyParent;
			helper.CreateMenuTemplatePivot("Parent", template, command);

			var childDummy1 = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var childCommand1 = helper.CreateDocCommand("Child1");
			childCommand1.Parent = childDummy1;
			helper.CreateMenuMenuPivot(command, childCommand1);
			helper.CreateMenuTemplatePivot("Child1", template, childCommand1);

			var childDummy2 = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var childCommand2 = helper.CreateDocCommand("Child2");
			childCommand2.Parent = childDummy2;
			helper.CreateMenuMenuPivot(command, childCommand2);
			helper.CreateMenuTemplatePivot("Child2", template, childCommand2);

			var grandDummy1 = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var grandChildCommand = helper.CreateDocCommand("Grandchild");
			grandChildCommand.Parent = grandDummy1;
			helper.CreateMenuMenuPivot(childCommand1, grandChildCommand);
			helper.CreateMenuTemplatePivot("Grandchild", template, grandChildCommand);

			Factory.Save();

			using (var printTask = new PrintTask())
			{
				var parentLoader = new PrintTaskDocumentPackLoader(printTask, command, null);
				parentLoader.LoadAll();

				var pack = printTask.GetDocumentPacks().First();
				AssertEquals("Should be four documents in pack", 4, pack.Count);

				AssertEquals(pack, ((Report)pack[0]).Parent);
				AssertEquals(pack, ((Report)pack[1]).Parent);
				AssertEquals(pack, ((Report)pack[2]).Parent);
				AssertEquals(pack, ((Report)pack[3]).Parent);

				AssertEquals(dummyParent, ((Report)pack[0]).Parent.BusinessObjectToLogAgainst);
				AssertEquals(dummyParent, ((Report)pack[1]).Parent.BusinessObjectToLogAgainst);
				AssertEquals(dummyParent, ((Report)pack[2]).Parent.BusinessObjectToLogAgainst);
				AssertEquals(dummyParent, ((Report)pack[3]).Parent.BusinessObjectToLogAgainst);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommands_ShouldSynchronizeLastTemplateGeneratorLanguage()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var template = helper.CreateTemplate(nameof(Core.Constants.DataContext.Shipment), "Template");
			var command = helper.CreateDocCommand("Parent");
			command.SU_IsDocPack = true;
			command.Parent = dummy;

			var childCommand1 = helper.CreateDocCommand("Child1");
			helper.CreateMenuMenuPivot(command, childCommand1);
			helper.CreateMenuTemplatePivot("Child1", template, childCommand1);

			Factory.Save();

			using (var printTask = new PrintTask())
			{
				var parentLoader = new PrintTaskDocumentPackLoader(printTask, command, null);
				parentLoader.LoadAll();

				var pack = printTask.GetDocumentPacks().First();
				AssertEquals(Core.SharedConstants.Languages.EnglishAmerican, pack.LastTemplateGeneratorLanguage);

				pack.RebuildIfLanguageChanged(new DeliveryInstructions { Language = Core.SharedConstants.Languages.ChineseSimplified });
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, pack.LastTemplateGeneratorLanguage);
			}
		}
	}
}
