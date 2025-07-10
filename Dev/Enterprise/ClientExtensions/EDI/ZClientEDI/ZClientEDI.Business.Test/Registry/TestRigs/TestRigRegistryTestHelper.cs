using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	public static class TestRigRegistryTestHelper
	{
		public static void AddTestRigOptions(string product, string productArea, string module, string changeType, string backupFile, string additionalOptions = "")
		{
			var registryItem = EDIDataRegistry.Instance.TestRigOptions.Value;
			var options = registryItem.OptionsCollection.AddNew();
			options.Product = product;
			options.ProductArea = productArea;
			options.Module = module;
			options.ChangeType = changeType;
			options.BackupFile = backupFile;
			options.AdditionalOptions = additionalOptions;

			EDIDataRegistry.Instance.TestRigOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);
		}

		public static void AddWorkItemTypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}
	}
}