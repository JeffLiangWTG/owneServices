using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ShipmentMilestoneEventUpdatesRegistryItemEditor))]
	sealed class ShipmentMilestoneEventUpdatesRegistryItemEditorTest : MilestoneEventUpdatesRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ShipmentMilestoneEventUpdatesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ShipmentMilestoneEventUpdatesRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ShipmentMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ShipmentMilestoneEventUpdatesCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ShipmentMilestoneEventUpdatesCollection();
			collection.AddNew("TST", "PY1", "PY2");
			return new object[] { collection };
		}

		#endregion
	}
}
