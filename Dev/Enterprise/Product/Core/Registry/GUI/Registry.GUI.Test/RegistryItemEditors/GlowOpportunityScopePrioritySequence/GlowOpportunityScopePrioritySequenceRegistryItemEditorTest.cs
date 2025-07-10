using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowOpportunityScopePrioritySequenceRegistryItemEditor))]
	sealed class GlowOpportunityScopePrioritySequenceRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new GlowOpportunityScopePrioritySequenceRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GlowOpportunityScopePrioritySequenceRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GlowOpportunityScopePrioritySequenceRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GlowOpportunityScopePrioritySequenceRegistryItem("", null, null, null, RegistryStorageFlags.System, new GlowOpportunityScopePrioritySequenceCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return [new GlowOpportunityScopePrioritySequenceCollection()];
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
