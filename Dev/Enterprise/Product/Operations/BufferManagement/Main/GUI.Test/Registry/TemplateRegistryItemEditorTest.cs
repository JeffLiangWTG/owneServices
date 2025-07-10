using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TemplateRegistryItemEditor))]
	public class TemplateRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TemplateRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TemplateRegistryItemEditor(new TemplateRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TemplateRegistryItemControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var header = new TemplateCriteria();
			return new object[] { header };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TemplateRegistryItemControl)editorPane).ReadOnly;
		}
	}
}
