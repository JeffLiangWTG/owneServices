using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AssistWithThisTaskSettingForOneWorkflowType))]
	public class AssistWithThisTaskSettingForOneWorkflowTest : RegistryBusinessObjectTest
	{
		public void TestSetCustomDefaultValues()
		{
			AssertEquals(10, BizObj.LowEstimateMinutes);
			AssertEquals(2, BizObj.VariationFactor);
		}

		public void TestXmlSerialiseDeserialiseFromExistingRecord()
		{
			BizObj.TaskType = "INV";
			BizObj.LowEstimateMinutes = 25;
			BizObj.VariationFactor = 3;

			var deserialised = WorkflowDataRegistryTestHelper.SerializeAndDeserialize(BizObj);

			AssertEquals("INV", deserialised.TaskType);
			AssertEquals(25, deserialised.LowEstimateMinutes);
			AssertEquals(3, deserialised.VariationFactor);
		}

		public void TestValidateTaskType()
		{
			AssertEquals("WKI", BizObj.Code);
			AssertEquals(ZString.Empty, BizObj.TaskType);
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.TaskTypeInfo);

			BizObj.TaskType = "INV";
			AssertNoErrors(BizObj.TaskTypeInfo);

			BizObj.TaskType = "CDF";
			AssertHasError(BizObj.TaskTypeInfo, "Enter a valid selection.");
		}

		public void TestValidateLowEstimateMinutes()
		{
			AssertEquals(10, BizObj.LowEstimateMinutes);
			AssertNoErrors(BizObj.LowEstimateMinutesInfo);

			BizObj.LowEstimateMinutes = -1;
			AssertHasError(BizObj.LowEstimateMinutesInfo, "Please enter a 'Low Estimate Minutes' within the range 0 to 999.");

			BizObj.LowEstimateMinutes = 0;
			AssertNoErrors("We should allow 0 in case a customer wants to ignore estimates etc.", BizObj.LowEstimateMinutesInfo);

			BizObj.LowEstimateMinutes = 1000;
			AssertHasError(BizObj.LowEstimateMinutesInfo, "Please enter a 'Low Estimate Minutes' within the range 0 to 999.");

			BizObj.LowEstimateMinutes = 999;
			AssertNoErrors(BizObj.LowEstimateMinutesInfo);
		}

		public void TestValidateVariationFactor()
		{
			AssertEquals(2, BizObj.VariationFactor);
			AssertNoErrors(BizObj.VariationFactorInfo);

			BizObj.VariationFactor = 0;
			AssertHasError(BizObj.VariationFactorInfo, "Please enter a 'Variation Factor' within the range 1 to 999.");

			BizObj.VariationFactor = 1;
			AssertNoErrors(BizObj.VariationFactorInfo);

			BizObj.VariationFactor = 1000;
			AssertHasError(BizObj.VariationFactorInfo, "Please enter a 'Variation Factor' within the range 1 to 999.");

			BizObj.VariationFactor = 999;
			AssertNoErrors(BizObj.VariationFactorInfo);
		}

		public void TestTaskTypeList_ShouldBeCompanySpecific()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;
			Factory.Save();

			var newCompanyTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypesForWorkflowType = newCompanyTaskTypes.AddNew();
			taskTypesForWorkflowType.Code = "WKI";
			var taskType = taskTypesForWorkflowType.TaskTypes.AddNew();
			taskType.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, newCompanyTaskTypes);

			var item = new AssistWithThisTaskSettingForOneWorkflowType { Code = "WKI" };
			var list = item.TaskTypeList.Cast<ICodeDescription>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder("The codes defined for this workflow type in this specific company should be included.", new[] { "INV" }, list);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				item = new AssistWithThisTaskSettingForOneWorkflowType { Code = "WKI" };
				list = item.TaskTypeList.Cast<ICodeDescription>().Select(x => x.Code);
				AssertContainsExactElementsInAnyOrder("The codes defined for this workflow type in this specific company should be included.", new[] { "CDF" }, list);
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BizObj.TaskType = "INV";
			BizObj.LowEstimateMinutes = 25;
			BizObj.VariationFactor = 3;

			return BizObj;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var definition = (AssistWithThisTaskSettingForOneWorkflowType)clone;

			AssertEquals("INV", definition.TaskType);
			AssertEquals(25, definition.LowEstimateMinutes);
			AssertEquals(3, definition.VariationFactor);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AssistWithThisTaskSettingForOneWorkflowType { Code = "WKI" };
		}

		new AssistWithThisTaskSettingForOneWorkflowType BizObj => (AssistWithThisTaskSettingForOneWorkflowType)base.BizObj;

		protected override void SetUp()
		{
			base.SetUp();

			SetUpTaskTypesForAssistWithThisTaskTests();
		}

		public static void SetUpTaskTypesForAssistWithThisTaskTests()
		{
			var taskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypesForWorkflowType = taskTypes.AddNew();
			taskTypesForWorkflowType.Code = "WKI";
			var taskType = taskTypesForWorkflowType.TaskTypes.AddNew();
			taskType.Code = "INV";

			taskTypesForWorkflowType = taskTypes.AddNew();
			taskTypesForWorkflowType.Code = "INQ";
			taskType = taskTypesForWorkflowType.TaskTypes.AddNew();
			taskType.Code = "CDF";

			taskTypesForWorkflowType = taskTypes.AddNew();
			taskTypesForWorkflowType.Code = "WKP";
			taskType = taskTypesForWorkflowType.TaskTypes.AddNew();
			taskType.Code = "INV";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taskTypes);
		}
	}
}
