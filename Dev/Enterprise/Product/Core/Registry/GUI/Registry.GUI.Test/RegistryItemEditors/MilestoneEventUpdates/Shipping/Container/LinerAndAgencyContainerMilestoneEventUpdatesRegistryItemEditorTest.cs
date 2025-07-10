using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(LinerAndAgencyContainerMilestoneEventUpdatesRegistryItemEditor))]
	sealed class LinerAndAgencyContainerMilestoneEventUpdatesRegistryItemEditorTest : MilestoneEventUpdatesRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdatesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LinerAndAgencyContainerMilestoneEventUpdatesRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new LinerAndAgencyContainerMilestoneEventUpdatesCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new LinerAndAgencyContainerMilestoneEventUpdatesCollection();
			collection.AddNew("TST", "PY1", "PY2");
			return new object[] { collection };
		}

		#endregion
	}
}
