using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business.Test
{
	public static class TestConfigsHelper
	{
		#region Test Configs

		public static SchematicTestConfig CreateSchematicTestConfig(BusinessObjectFactory factory, string[] workflowTypes, string systemName = "WTGDEV", bool shouldUseExistingSystem = false)
		{
			return SchematicTestConfig.Create(factory, workflowTypes, systemName, shouldUseExistingSystem);
		}

		public static SchematicTestConfig CreateSchematicTestConfig(BusinessObjectFactory factory, string workflowType = "ORG", string systemName = "WTGDEV", bool shouldUseExistingSystem = false)
		{
			return CreateSchematicTestConfig(factory, new[] { workflowType }, systemName, shouldUseExistingSystem);
		}

		public static VisualBoardTestConfig CreateVisualBoardTestConfig(BusinessObjectFactory factory, string[] workflowTypes, string systemName = "WTGDEV", bool shouldUseExistingSystem = false)
		{
			return VisualBoardTestConfig.Create(factory, workflowTypes, systemName, shouldUseExistingSystem);
		}

		public static VisualBoardTestConfig CreateVisualBoardTestConfig(BusinessObjectFactory factory, string workflowType = "ORG", bool shouldUseExistingSystem = false)
		{
			return CreateVisualBoardTestConfig(factory, new[] { workflowType }, "WTGDEV", shouldUseExistingSystem);
		}

		public static ConstrainedSchematicTestConfig CreateConstrainedSchematicTestConfig(BusinessObjectFactory factory, string workflowType = "ORG", bool makeResourcesPartOfReleaseGroup = true, bool shouldUseExistingSystem = false)
		{
			return ConstrainedSchematicTestConfig.Create(factory, workflowType, makeResourcesPartOfReleaseGroup, shouldUseExistingSystem);
		}

		public static ComplexConstrainedSchematicTestConfig CreateComplexConstrainedSchematicTestConfig(BusinessObjectFactory factory, string workflowType = "ORG", bool makeResourcesPartOfReleaseGroup = true, bool createWorkflowsAndTasks = true, bool shouldUseExistingSystem = false)
		{
			return ComplexConstrainedSchematicTestConfig.Create(factory, workflowType, makeResourcesPartOfReleaseGroup, createWorkflowsAndTasks, shouldUseExistingSystem);
		}

		public static TagsTestConfig CreateTagsTestConfig(BusinessObjectFactory factory, string workflowType = "ORG", bool shouldUseExistingSystem = false)
		{
			return TagsTestConfig.Create(factory, workflowType, shouldUseExistingSystem);
		}

		#endregion
	}
}
