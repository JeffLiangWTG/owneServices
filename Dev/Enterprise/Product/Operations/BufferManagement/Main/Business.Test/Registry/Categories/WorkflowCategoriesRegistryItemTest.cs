namespace Enterprise.BufferManagement.Business.Testing
{
	using Enterprise.Integration;
	using Enterprise.Registry.Business.Testing;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Environment.Testing;
	using NUnit.Framework;

	[TestedType(typeof(WorkflowCategoriesRegistryItem))]
	class WorkflowCategoriesRegistryItemTest : StronglyTypedRegistryItemTestCase<CategorisedWorkflowCategoriesCollection>
	{
		public void TestProperties()
		{
			AssertNotNull(Item.DefaultValue);
			AssertEquals(typeof(WorkflowCategoriesRegistryDataType), Item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<CategorisedWorkflowCategoriesCollection, CategorisedWorkflowCategoriesCollection> GetNewRegistryItem()
			=> new WorkflowCategoriesRegistryItem("", null, null, null, RegistryStorageFlags.System);
	}

	[TestedType(typeof(WorkflowCategoriesRegistryDataType))]
	class WorkflowCategoriesRegistryDataTypeTest : NonPersistentBusinessObjectCollectionRegistryDataTypeTestCase<WorkflowCategoriesRegistryDataType, CategorisedWorkflowCategoriesCollection>
	{
		protected override WorkflowCategoriesRegistryDataType GetNewDataType() => new WorkflowCategoriesRegistryDataType();

		protected override string ExpectedEditorName => "WorkflowCategoriesRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples() => new[]
			{
				GetValidSample("WRK", "Work Item", "ERR", "Logic Error", "STY", "Style Error"),
				GetValidSample("WR2", "Work Item2", "ER2", "Logic Error2", "ST2", "Style Error2"),
			};

		ValidSampleAndBinaryValueInDB GetValidSample(string parentCode, string parentDesc, string codeTaskCode, string codeTaskDesc, string revTaskCode, string revTaskDesc)
		{
			var collection = new CategorisedWorkflowCategoriesCollection();
			var parent = collection.AddNew();
			parent.Code = parentCode;
			parent.Description = (NoResString)parentDesc;

			var codingTask = parent.Categories.AddNew();
			codingTask.Code = codeTaskCode;
			codingTask.Description = (NoResString)codeTaskDesc;

			var reviewTask = parent.Categories.AddNew();
			reviewTask.Code = revTaskCode;
			reviewTask.Description = (NoResString)revTaskDesc;

			var sampleEncoded =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
					"<ArrayOfCategorisedWorkflowCategories xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
						"<CategorisedWorkflowCategories>" +
							"<CodeMaxLength>3</CodeMaxLength>" +
							$"<Code>{parentCode}</Code>" +
							$"<Description>{parentDesc}</Description>" +
							"<ArrayOfWorkflowCategory>" +
								"<WorkflowCategory>" +
									"<CodeMaxLength>3</CodeMaxLength>" +
									$"<Code>{codeTaskCode}</Code>" +
									$"<Description>{codeTaskDesc}</Description>" +
								"</WorkflowCategory>" +
								"<WorkflowCategory>" +
									"<CodeMaxLength>3</CodeMaxLength>" +
									$"<Code>{revTaskCode}</Code>" +
									$"<Description>{revTaskDesc}</Description>" +
								"</WorkflowCategory>" +
							"</ArrayOfWorkflowCategory>" +
						"</CategorisedWorkflowCategories>" +
					"</ArrayOfCategorisedWorkflowCategories>";
			var byteArrayValue = System.Text.Encoding.Unicode.GetBytes(sampleEncoded);
			return new ValidSampleAndBinaryValueInDB(collection, byteArrayValue);
		}
	}
}
