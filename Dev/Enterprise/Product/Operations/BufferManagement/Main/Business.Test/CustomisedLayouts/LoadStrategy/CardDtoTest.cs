using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(CardDto))]
	public class CardDtoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "second time == the weird stuff");
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 7, description: "Your mother");

			var controlCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line = controlCustomisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = ProcessTasksSchema.Constants.P9_Description;

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;
			var strategy = new ControlCustomisationLinePropertyCache(line, new ProcessTask[] { task }, Enumerable.Empty<ProcessHeader>());
			var definitions = new TagDefinitionCache(Factory);

			return (BusinessObject)new FactorylessCardContent(workflow, task, viewModel, new CustomisedControlDataCache(), definitions, new PopulateTaskCardStrategy()).Bindable;
		}

		public void TestDuplicateFields_ShouldNotThrowExceptions()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Here's a workflow. Perhaps you'd like to complete its tasks.", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 10, description: "Sacré bleu, I peed in m'pants.");

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line1 = customisation.CustomisationLines.AddNew();
			line1.PropertySource = PropertySourceList.Codes.ProcessTask;
			line1.PropertyName = "P9_Description";
			var line2 = customisation.CustomisationLines.AddNew();
			line2.PropertySource = PropertySourceList.Codes.ProcessTask;
			line2.PropertyName = "P9_Description";
			BMSTestHelper.CreateControlCustomisationLink(Factory, config.BufferBoard, customisation, "INQ");

			Factory.Save();

			var cacheCollection = new[]
			{
				new ControlCustomisationLinePropertyCache(line1, new[] { task }, new[] { workflow }),
				new ControlCustomisationLinePropertyCache(line2, new[] { task }, new[] { workflow }),
			};

			var content = new TaskCardContent(task, BMSTestHelper.CreateViewModel(config.BufferSection));
			CardDto dto = null;

			AssertNoExceptionThrown(() => dto = new CardDto(content, cacheCollection));
			AssertEquals("Sacré bleu, I peed in m'pants.", dto["TSK_P9_Description"]);
		}
	}
}
