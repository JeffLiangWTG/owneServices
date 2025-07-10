using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(IssueWorkItemCreationThresholdRegistryEditor))]
	class IssueWorkItemCreationThresholdRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new IssueWorkItemCreationThresholdRegistryEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((IssueWorkItemCreationThresholdControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(IssueWorkItemCreationThresholdControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new IssueWorkItemCreationThresholdRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new IssueWorkItemCreationThresholdCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory)
			{ new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 15, ThresholdTimespan = 22 }, new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 1, ThresholdTimespan = 23121995 }, };
			return new[] { collection };
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
