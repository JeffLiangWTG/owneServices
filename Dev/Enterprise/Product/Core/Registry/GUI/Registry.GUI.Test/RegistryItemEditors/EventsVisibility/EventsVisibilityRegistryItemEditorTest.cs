using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EventsVisibilityRegistryItemEditor))]
	sealed class EventsVisibilityRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EventsVisibilityRegistryControl)editorPane).ReadOnly;
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EventVisibilityRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new EventVisibilityCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EventsVisibilityRegistryItemEditor(new EventVisibilityRegistryItemDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EventsVisibilityRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new EventVisibilityCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
