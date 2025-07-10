using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(IncidentEmailTemplatePairRegistryEditor))]
	public class IncidentEmailTemplatePairRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new IncidentEmailTemplatePairRegistryEditor(new IncidentEmailTemplatePairRegistryDataType(new IncidentEmailTemplatePair(typeof(DocSupportIncident))), null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((IncidentEmailTemplatePairRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(IncidentEmailTemplatePairRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new IncidentEmailTemplatePairRegistryItem("", null, null, null, RegistryStorageFlags.System, new IncidentEmailTemplatePair(typeof(DocSupportIncident)));
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override object[] GetValidRegistryValues()
		{
			var template = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject one", "Email body template one");
			var templatePair = new IncidentEmailTemplatePair(typeof(DocSupportIncident), template, template);
			var emptyTemplatePair = new IncidentEmailTemplatePair(typeof(DocSupportIncident));
			return new object[] { templatePair, emptyTemplatePair };
		}
	}
}
