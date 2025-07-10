using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AssistWithThisTaskRegistryDataType))]
	sealed class AssistWithThisTaskRegistryDataTypeTest : NonPersistentBusinessObjectCollectionRegistryDataTypeTestCase<AssistWithThisTaskRegistryDataType, CategorisedAssistWithThisTaskSettingCollection>
	{
		protected override AssistWithThisTaskRegistryDataType GetNewDataType()
		{
			return new AssistWithThisTaskRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "WorkflowManagerAssistWithThisTaskRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new CategorisedAssistWithThisTaskSettingCollection();
			var wkiCategory = collection1.AddNew();
			wkiCategory.Code = "WKI";
			wkiCategory.Setting.Code = "WKI";
			wkiCategory.Setting.TaskType = "INV";
			wkiCategory.Setting.LowEstimateMinutes = 30;
			wkiCategory.Setting.VariationFactor = 9;

			var shpCategory = collection1.AddNew();
			shpCategory.Code = "INQ";
			shpCategory.Setting.Code = "INQ";
			shpCategory.Setting.TaskType = "CDF";

			var collection2 = new CategorisedAssistWithThisTaskSettingCollection();
			var wkpCategory = collection2.AddNew();
			wkpCategory.Code = "WKP";
			wkpCategory.Setting.Code = "WKP";
			wkpCategory.Setting.TaskType = "INV";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfCategorisedAssistWithThisTaskSetting xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CategorisedAssistWithThisTaskSetting><CodeMaxLength>3</CodeMaxLength><Code>WKI</Code><Description /><AssistWithThisTaskSettingForOneWorkflowType><CodeMaxLength>3</CodeMaxLength><Code>WKI</Code><Description /><TaskType>INV</TaskType><LowEstimateMinutes>30</LowEstimateMinutes><VariationFactor>9</VariationFactor></AssistWithThisTaskSettingForOneWorkflowType></CategorisedAssistWithThisTaskSetting><CategorisedAssistWithThisTaskSetting><CodeMaxLength>3</CodeMaxLength><Code>INQ</Code><Description /><AssistWithThisTaskSettingForOneWorkflowType><CodeMaxLength>3</CodeMaxLength><Code>INQ</Code><Description /><TaskType>CDF</TaskType><LowEstimateMinutes>10</LowEstimateMinutes><VariationFactor>2</VariationFactor></AssistWithThisTaskSettingForOneWorkflowType></CategorisedAssistWithThisTaskSetting></ArrayOfCategorisedAssistWithThisTaskSetting>"),
				new ValidSampleAndBinaryValueInDB(collection2, @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfCategorisedAssistWithThisTaskSetting xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CategorisedAssistWithThisTaskSetting><CodeMaxLength>3</CodeMaxLength><Code>WKP</Code><Description /><AssistWithThisTaskSettingForOneWorkflowType><CodeMaxLength>3</CodeMaxLength><Code>WKP</Code><Description /><TaskType>INV</TaskType><LowEstimateMinutes>10</LowEstimateMinutes><VariationFactor>2</VariationFactor></AssistWithThisTaskSettingForOneWorkflowType></CategorisedAssistWithThisTaskSetting></ArrayOfCategorisedAssistWithThisTaskSetting>")
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			AssistWithThisTaskSettingForOneWorkflowTest.SetUpTaskTypesForAssistWithThisTaskTests();
		}
	}
}
