using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(QualityIterationAssignmentRegistryEditor))]
	public class QualityIterationAssignmentRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new QualityIterationAssignmentRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new QualityIterationAssignmentRegistryEditor(new QualityIterationAssignmentRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(QualityIterationAssignmentControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var header = new QualityIterationAssignmentHeader();
			return new object[] { header };
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
			return !((QualityIterationAssignmentControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
