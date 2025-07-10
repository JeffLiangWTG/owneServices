using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EventsVisibilityOverrideRegistryItemEditor))]
	sealed class EventsVisibilityOverrideRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestSetValueFromEditorPane()
		{
			using (ZForm form = new ZForm())
			{
				EventsVisibilityOverrideRegistryControl editorPane = (EventsVisibilityOverrideRegistryControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				EventVisibilityOverrideCollection collection = new EventVisibilityOverrideCollection();

				EventVisibilityOverride parent1 = collection.AddNew();
				EventVisibilityOverride parent2 = collection.AddNew();
				EventVisibilityOverride parent3 = collection.AddNew();

				EventVisibilityOverrideSetting child1 = parent1.EventVisibilityOverrideSettings.AddNew();
				EventVisibilityOverrideSetting child2 = parent2.EventVisibilityOverrideSettings.AddNew();
				EventVisibilityOverrideSetting child3 = parent3.EventVisibilityOverrideSettings.AddNew();

				parent1.Code = "123";
				parent2.Code = "ABC";
				parent3.Code = "!@#";

				child1.Code = "C1";
				child2.Code = "C2";
				child3.Code = "C3";

				Editor.SetValueFromEditorPane(editorPane, collection);
				AssertSetAndGetValuesEqual(collection, ((IDataBoundControl)editorPane).DataSource);
			}
		}
		#region Implementation

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EventsVisibilityOverrideRegistryControl)editorPane).ReadOnly;
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EventVisibilityOverrideRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new EventVisibilityOverrideCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EventsVisibilityOverrideRegistryItemEditor(new EventVisibilityOverrideRegistryItemDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EventsVisibilityOverrideRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new EventVisibilityOverrideCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
