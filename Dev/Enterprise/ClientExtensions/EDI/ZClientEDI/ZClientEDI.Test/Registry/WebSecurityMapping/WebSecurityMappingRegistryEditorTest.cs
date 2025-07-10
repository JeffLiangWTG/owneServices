using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(WebSecurityMappingRegistryEditor))]
	public class WebSecurityMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebSecurityMappingRegistryItem("", null, null, null, new WebSecurityMappingRegistryEditorInfo(), RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new WebSecurityMappingRegistryEditor(new WebSecurityMappingRegistryDataType());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebSecurityMappingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			WebSecurityMappingCollection collection = new WebSecurityMappingCollection();
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ENT", "ACC");
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ENT", "SYS");
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
			return !((WebSecurityMappingControl)editorPane).ReadOnly;
		}
		#endregion
	}
}
