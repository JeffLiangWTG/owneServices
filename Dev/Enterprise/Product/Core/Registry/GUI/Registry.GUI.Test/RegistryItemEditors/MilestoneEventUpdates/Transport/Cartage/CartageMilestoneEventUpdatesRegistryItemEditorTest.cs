using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CartageMilestoneEventUpdatesRegistryItemEditor))]
	sealed class CartageMilestoneEventUpdatesRegistryItemEditorTest : MilestoneEventUpdatesRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CartageMilestoneEventUpdatesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CartageMilestoneEventUpdatesRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CartageMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CartageMilestoneEventUpdatesCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CartageMilestoneEventUpdatesCollection();
			collection.AddNew("TST", "PY1", "PY2");
			return new object[] { collection };
		}

		#endregion
	}
}
