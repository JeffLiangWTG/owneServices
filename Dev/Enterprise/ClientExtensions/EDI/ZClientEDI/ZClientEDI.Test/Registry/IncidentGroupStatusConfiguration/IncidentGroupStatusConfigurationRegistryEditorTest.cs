using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(IncidentGroupStatusConfigurationRegistryEditor))]
	internal class IncidentGroupStatusConfigurationRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new IncidentGroupStatusConfigurationRegistryEditor(
				new IncidentGroupStatusConfigurationDataType(),
				new FallbackLevel(Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment),
				Factory);
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
			return !((IncidentGroupStatusConfigurationControl)editorPane).IsReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(IncidentGroupStatusConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new IncidentGroupStatusConfigurationRegistryItem(
				"IncidentGroupStatusConfigurationForTest",
				(NoResString)"IncidentGroupStatusConfigurationForTest",
				(NoResString)"Incident Group Stage Configuration(ForTest)",
				(NoResString)"Setup stage sequence, naming and behaviour for Incident Management Groups.(ForTest)",
				new IncidentGroupTypeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new IncidentGroupTypeCollection();
			collection.Add(new IncidentGroupType() { GroupType = "AAA" });
			collection.Add(new IncidentGroupType() { GroupType = "BBB" });

			return new object[] { collection };
		}
	}
}
