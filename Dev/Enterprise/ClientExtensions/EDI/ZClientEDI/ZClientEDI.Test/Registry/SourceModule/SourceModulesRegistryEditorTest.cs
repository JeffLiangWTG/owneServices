using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(SourceModulesRegistryEditor))]
	public class SourceModulesRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SourceModulesRegistryItem("", null, null, null, RegistryStorageFlags.System, new SourceModuleCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new SourceModulesRegistryEditor(new SourceModulesRegistryDataType(), new SourceModulesRegistryEditorInfo());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SourceModulesControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new SourceModuleCollection();
			collection.AddNew("DummyMenuItem1", "Dummy Menu Item 1", "Dummies > House", ModuleListType.MenuSection, "HOU", true, false);
			collection.AddNew("DummyMenuItem2", "Dummy Menu Item 2", "Dummies > Boat", ModuleListType.MenuSection, "BOA", false, true);
			return new object[] { collection };
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
			return !((SourceModulesControl)editorPane).ReadOnly;
		}
		#endregion
	}
}
