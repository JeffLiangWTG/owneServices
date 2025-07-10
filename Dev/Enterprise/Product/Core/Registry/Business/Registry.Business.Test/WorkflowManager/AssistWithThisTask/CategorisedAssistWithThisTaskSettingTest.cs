using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CategorisedAssistWithThisTaskSetting))]
	sealed class CategorisedAssistWithThisTaskSettingTest : RegistryBusinessObjectTest
	{
		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var clonedCategorised = (CategorisedAssistWithThisTaskSetting)clone;
			AssertEquals(5, clonedCategorised.CodeMaxLength);
			AssertEquals("WKI", clonedCategorised.Code);
			AssertEquals("Work Item", clonedCategorised.Description);

			var clonedSetting = clonedCategorised.Setting;

			AssertEquals("WKI", clonedSetting.Code);
			AssertEquals("INV", clonedSetting.TaskType);
			AssertEquals(20, clonedSetting.LowEstimateMinutes);
			AssertEquals(3, clonedSetting.VariationFactor);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var toClone = (CategorisedAssistWithThisTaskSetting)GetNewBusinessObject();

			toClone.CodeMaxLength = 5;
			toClone.Code = "WKI";
			toClone.Description = (NoResString)"Work Item";

			toClone.Setting.TaskType = "INV";
			toClone.Setting.LowEstimateMinutes = 20;
			toClone.Setting.VariationFactor = 3;

			return toClone;
		}

		public void TestXmlSerializeDeserializeFromExistingRecord()
		{
			BizObj.Code = "WKI";
			BizObj.Setting.TaskType = "INV";
			BizObj.Setting.LowEstimateMinutes = 25;
			BizObj.Setting.VariationFactor = 3;

			var deserialized = WorkflowDataRegistryTestHelper.SerializeAndDeserialize(BizObj);

			AssertEquals("WKI", deserialized.Code);
			AssertEquals("WKI", deserialized.Setting.Code);
			AssertEquals("INV", deserialized.Setting.TaskType);
			AssertEquals(25, deserialized.Setting.LowEstimateMinutes);
			AssertEquals(3, deserialized.Setting.VariationFactor);
		}

		public void TestValidation_WhenSettingHasErrors_ShouldThrowException()
		{
			BizObj.Code = "WKI";
			BizObj.Setting.TaskType = "CKD";
			AssertHasError(BizObj.Setting.TaskTypeInfo, "Enter a valid selection.");
			AssertExceptionThrown<RegistryValidationException>(() => BizObj.RunPreSaveValidation());
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		new CategorisedAssistWithThisTaskSetting BizObj
		{
			get { return (CategorisedAssistWithThisTaskSetting)base.BizObj; }
		}

		#region Overrides of Test

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			AssistWithThisTaskSettingForOneWorkflowTest.SetUpTaskTypesForAssistWithThisTaskTests();
		}
	}
}
