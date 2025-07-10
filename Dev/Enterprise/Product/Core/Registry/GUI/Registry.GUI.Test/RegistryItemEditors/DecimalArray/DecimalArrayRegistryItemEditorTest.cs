using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DecimalArrayRegistryItemEditor))]
	sealed class DecimalArrayRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DecimalArrayRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DecimalArrayRegistryItemEditor(new DecimalArrayRegistryDataType());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DecimalArrayControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[]
			{
				Array.Empty<decimal>(),
				new decimal[] { 12.23m, 21m }
			};
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DecimalArrayControl)editorPane).ReadOnly;
		}
	}
}
