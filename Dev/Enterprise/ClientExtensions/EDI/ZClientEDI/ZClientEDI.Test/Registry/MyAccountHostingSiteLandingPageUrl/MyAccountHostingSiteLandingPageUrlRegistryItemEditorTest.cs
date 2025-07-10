using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(MyAccountHostingSiteLandingPageUrlRegistryItemEditor))]
	public class MyAccountHostingSiteLandingPageUrlRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((MyAccountHostingSiteLandingPageUrlUserControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return Enterprise.Registry.GUI.RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override object[] GetValidRegistryValues()
		{
			return new MyAccountHostingSiteLandingPageUrlCollection[] { new MyAccountHostingSiteLandingPageUrlCollection() };
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(MyAccountHostingSiteLandingPageUrlUserControl);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new MyAccountHostingSiteLandingPageUrlRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MyAccountHostingSiteLandingPageUrlRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new MyAccountHostingSiteLandingPageUrlCollection());
		}
	}
}
