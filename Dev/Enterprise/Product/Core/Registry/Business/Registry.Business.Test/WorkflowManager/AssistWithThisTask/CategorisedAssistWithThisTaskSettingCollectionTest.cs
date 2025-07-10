using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CategorisedAssistWithThisTaskSettingCollection))]
	sealed class CategorisedAssistWithThisTaskSettingCollectionTest : RegistryBusinessObjectCollectionTestCase<CategorisedAssistWithThisTaskSettingCollection>
	{
		public void TestGetTaskDetailsForWorkflowType()
		{
			AssistWithThisTaskSettingForOneWorkflowTest.SetUpTaskTypesForAssistWithThisTaskTests();

			var categories = new CategorisedAssistWithThisTaskSettingCollection(true);

			var wkiCategory = (CategorisedAssistWithThisTaskSetting)categories.FindByCode("WKI");
			wkiCategory.Setting.TaskType = "INV";
			wkiCategory.Setting.LowEstimateMinutes = 15;

			var shpCategory = (CategorisedAssistWithThisTaskSetting)categories.FindByCode("SHP");
			shpCategory.Setting.TaskType = "UDF";
			shpCategory.Setting.VariationFactor = 4;

			AssertEquals("INV", categories.GetTaskDetailsForWorkflowType("WKI").TaskType);
			AssertEquals(15, categories.GetTaskDetailsForWorkflowType("WKI").LowEstimateMinutes);
			AssertEquals(2, categories.GetTaskDetailsForWorkflowType("WKI").VariationFactor);

			AssertEquals("UDF", categories.GetTaskDetailsForWorkflowType("SHP").TaskType);
			AssertEquals(10, categories.GetTaskDetailsForWorkflowType("SHP").LowEstimateMinutes);
			AssertEquals(4, categories.GetTaskDetailsForWorkflowType("SHP").VariationFactor);

			AssertEquals(ZString.Empty, categories.GetTaskDetailsForWorkflowType("CON").TaskType);
		}

		public void TestSynchroniseWithWorkflowDescriptorListWillAddTaskDescription()
		{
			Collection.RemoveAll();
			AssertEquals(0, Collection.Count);

			Collection.SynchroniseWithWorkflowDescriptorList();
			var workflowDescriptorList = WorkflowDataRegistryHelper.GetWorkflowDescriptorList();

			var expectedDescriptions = workflowDescriptorList.ToArray().Select(x => x.Description);
			var actualDescriptions = Collection.Select(x => (string)((RegistryBusinessObject)x).Description);
			AssertContainsExactElementsInExactOrder("Descriptions should be added", expectedDescriptions, actualDescriptions);
		}

		protected override CategorisedAssistWithThisTaskSettingCollection GetCollectionToTest()
		{
			return new CategorisedAssistWithThisTaskSettingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CategorisedAssistWithThisTaskSetting();
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
	}
}
