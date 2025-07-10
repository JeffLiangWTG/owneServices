using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StringArrayRegistryItemEditor))]
	sealed class StringArrayRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StringArrayRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new StringArrayRegistryItemEditor(new DelimitedStringArrayRegistryDataType());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(StringArrayControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[]
			{
				Array.Empty<string>(),
				new string[] { "hello world", "goodbye world" }
			};
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((StringArrayControl)editorPane).ReadOnly;
		}
	}
}
