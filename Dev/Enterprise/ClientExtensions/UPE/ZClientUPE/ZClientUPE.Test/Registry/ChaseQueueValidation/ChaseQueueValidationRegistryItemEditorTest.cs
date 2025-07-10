using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(ChaseQueueValidationRegistryItemEditor))]
	internal class ChaseQueueValidationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ChaseQueueValidationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ChaseQueueValidationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ChaseQueueValidationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ChaseQueueValidationRegistryItem("", "", "", "");
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { new ChaseQueueValidationCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
	}
}
