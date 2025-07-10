using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor))]
	public class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor(new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryDataType(new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident))), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident)));
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
			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var template = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject one", "Email body template one");
			var templatePair = collection.AddNew();
			templatePair.Code = "AAA";
			templatePair.Description = (NoResString)"Template AAA";
			templatePair.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident), template, template);
			var noTemplatePair = collection.AddNew();
			noTemplatePair.Code = "BBB";
			noTemplatePair.Description = (NoResString)"Tempalte BBB";
			noTemplatePair.EmailTemplates = new IncidentEmailTemplatePair(typeof(DocSupportIncident));
			return new object[] { collection };
		}
	}
}
