using System;
using Enterprise.Integration;
using Enterprise.ProcessManagement.GUI.Test;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(IncidentClosureDispositionRegistryEditor))]
	public class IncidentClosureDispositionRegistryEditorTest : CodeDescriptionBoolTreeRegistryEditorTest
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new IncidentClosureDispositionRegistryEditor(RegistryItem.DataType, RegistryItem.EditorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(IncidentClosureDispositionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var editorInfo = new IncidentClosureDispositionRegistryEditorInfo(new MultilingualString[]
							{
								(NoResString)"Stage",
								(NoResString)"Criticality",
								(NoResString)"Product",
								(NoResString)"Disposition"
							},
							(NoResString)"Enabled",
							null, true, false, new bool[] { false, false, false, true });
			return new IncidentClosureDispositionRegistryItem("", null, null, null, RegistryStorageFlags.System, editorInfo, new IncidentClosureDispositionCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var result = new IncidentClosureDispositionCollection();
			return new object[] { result };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((IncidentClosureDispositionControl)editorPane).ReadOnly;
		}
	}
}
