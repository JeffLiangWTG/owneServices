using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BillOfLadingMilestoneEventUpdatesRegistryItemEditor))]
	sealed class BillOfLadingMilestoneEventUpdatesRegistryItemEditorTest : MilestoneEventUpdatesRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new BillOfLadingMilestoneEventUpdatesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BillOfLadingMilestoneEventUpdatesRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BillOfLadingMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new BillOfLadingMilestoneEventUpdatesCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new BillOfLadingMilestoneEventUpdatesCollection();
			collection.AddNew("TST", "PY1", "PY2");
			return new object[] { collection };
		}

		#endregion
	}
}
