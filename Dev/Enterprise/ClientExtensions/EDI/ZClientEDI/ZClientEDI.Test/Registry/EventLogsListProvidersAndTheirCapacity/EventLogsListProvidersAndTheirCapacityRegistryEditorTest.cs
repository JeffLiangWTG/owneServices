using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(EventLogsListProvidersAndTheirCapacityRegistryEditor))]
	public class EventLogsListProvidersAndTheirCapacityRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EventLogsListProvidersAndTheirCapacityRegistryItem("EventLogsListProvidersAndTheirCapacity", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EventLogsListProvidersAndTheirCapacityRegistryEditor(new EventLogsListProvidersAndTheirCapacityDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EventLogsListProvidersAndTheirCapacityControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var items = new EventLogsListProvidersAndTheirCapacityCollection[1];
			items[0] = new EventLogsListProvidersAndTheirCapacityCollection();
			var item = items[0].AddNewOrGetExisting("CargoWise One", 3);
			return items;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EventLogsListProvidersAndTheirCapacityControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
