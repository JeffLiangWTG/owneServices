using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowTaskTypesRegistryDataType))]
	sealed class WorkflowTaskTypesRegistryDataTypeTest : NonPersistentBusinessObjectCollectionRegistryDataTypeTestCase<WorkflowTaskTypesRegistryDataType, CategorisedWorkflowTaskTypesCollection>
	{
		protected override WorkflowTaskTypesRegistryDataType GetNewDataType()
		{
			return new WorkflowTaskTypesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "WorkflowManagerTaskTypesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new CategorisedWorkflowTaskTypesCollection();
			var parent = collection.AddNew();
			parent.Code = "WRK";
			parent.Description = (NoResString)"Work Item";

			var codingTask = parent.TaskTypes.AddNew();
			codingTask.Code = "COD";
			codingTask.Description = (NoResString)"Coding";
			codingTask.CreatesAppointment = true;

			var reviewTask = parent.TaskTypes.AddNew();
			reviewTask.Code = "RVW";
			reviewTask.Description = (NoResString)"Review";
			reviewTask.CanCloseTaskNotAssignedToSelf = false;

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,105,0,115,0,101,0,100,0,87,0,111,0,114,0,107,0,102,0,108,0,111,
0,119,0,84,0,97,0,115,0,107,0,84,0,121,0,112,0,101,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,
0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,
0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,
0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,105,0,115,0,101,0,100,0,87,0,111,0,114,0,107,0,102,0,108,0,111,0,119,0,84,0,97,
0,115,0,107,0,84,0,121,0,112,0,101,0,115,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,
0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,87,0,82,0,75,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,
0,111,0,110,0,62,0,87,0,111,0,114,0,107,0,32,0,73,0,116,0,101,0,109,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,
0,102,0,87,0,111,0,114,0,107,0,102,0,108,0,111,0,119,0,84,0,97,0,115,0,107,0,84,0,121,0,112,0,101,0,62,0,60,0,87,0,111,0,114,0,107,0,102,0,108,0,111,0,119,0,84,0,97,0,115,0,107,0,84,0,121,
0,112,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,
0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,67,0,79,0,68,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,67,0,111,
0,100,0,105,0,110,0,103,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,
0,67,0,97,0,110,0,67,0,108,0,111,0,115,0,101,0,84,0,97,0,115,0,107,0,78,0,111,0,116,0,65,0,115,0,115,0,105,0,103,0,110,0,101,0,100,0,84,0,111,0,83,0,101,0,108,0,102,0,62,0,89,0,60,0,47,
0,67,0,97,0,110,0,67,0,108,0,111,0,115,0,101,0,84,0,97,0,115,0,107,0,78,0,111,0,116,0,65,0,115,0,115,0,105,0,103,0,110,0,101,0,100,0,84,0,111,0,83,0,101,0,108,0,102,0,62,0,60,0,47,0,87,
0,111,0,114,0,107,0,102,0,108,0,111,0,119,0,84,0,97,0,115,0,107,0,84,0,121,0,112,0,101,0,62,0,60,0,87,0,111,0,114,0,107,0,102,0,108,0,111,0,119,0,84,0,97,0,115,0,107,0,84,0,121,0,112,0,101,
0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,
0,60,0,67,0,111,0,100,0,101,0,62,0,82,0,86,0,87,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,82,0,101,0,118,0,105,
0,101,0,119,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,78,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,67,0,97,
0,110,0,67,0,108,0,111,0,115,0,101,0,84,0,97,0,115,0,107,0,78,0,111,0,116,0,65,0,115,0,115,0,105,0,103,0,110,0,101,0,100,0,84,0,111,0,83,0,101,0,108,0,102,0,62,0,78,0,60,0,47,0,67,0,97,
0,110,0,67,0,108,0,111,0,115,0,101,0,84,0,97,0,115,0,107,0,78,0,111,0,116,0,65,0,115,0,115,0,105,0,103,0,110,0,101,0,100,0,84,0,111,0,83,0,101,0,108,0,102,0,62,0,60,0,47,0,87,0,111,0,114,
0,107,0,102,0,108,0,111,0,119,0,84,0,97,0,115,0,107,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,87,0,111,0,114,0,107,0,102,0,108,0,111,0,119,0,84,0,97,
0,115,0,107,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,105,0,115,0,101,0,100,0,87,0,111,0,114,0,107,0,102,0,108,0,111,0,119,0,84,0,97,0,115,0,107,0,84,
0,121,0,112,0,101,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,105,0,115,0,101,0,100,0,87,0,111,0,114,0,107,0,102,0,108,0,111,0,119,
0,84,0,97,0,115,0,107,0,84,0,121,0,112,0,101,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		public void TestTranslatableTaskTypeDescription()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = new WorkflowTaskTypesRegistryItem(
						"ProcessManagerTaskTypes",
						RawDataRegistry.Categories.WorkflowManager,
						null,
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company);

				var collection = registryItem.Value;
				var workflowTaskTypeCollection = collection.AddNew();
				workflowTaskTypeCollection.Code = "WFT";
				workflowTaskTypeCollection.EnglishDescription = "Work Flow Task Type Collection";

				var taskType = workflowTaskTypeCollection.TaskTypes.AddNew();
				taskType.Code = "TST";
				taskType.EnglishDescription = "Testing Type";

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				var key = ((ResourceString)registryItem.Value[0].TaskTypes[0].Description).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "测试"));

				AssertNotEquals("Testing Type", registryItem.Value[0].TaskTypes[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
				AssertEquals("测试", registryItem.Value[0].TaskTypes[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}
	}
}
