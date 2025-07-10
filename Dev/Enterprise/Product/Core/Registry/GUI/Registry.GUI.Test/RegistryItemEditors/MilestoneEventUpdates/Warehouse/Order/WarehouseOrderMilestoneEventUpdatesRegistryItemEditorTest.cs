using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WarehouseOrderMilestoneEventUpdatesRegistryItemEditor))]
	sealed class WarehouseOrderMilestoneEventUpdatesRegistryItemEditorTest : MilestoneEventUpdatesRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WarehouseOrderMilestoneEventUpdatesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WarehouseOrderMilestoneEventUpdatesRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WarehouseOrderMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new WarehouseOrderMilestoneEventUpdatesCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new WarehouseOrderMilestoneEventUpdatesCollection();
			collection.AddNew("TST", "PY1", "PY2");
			return new object[] { collection };
		}

		#endregion
	}
}
