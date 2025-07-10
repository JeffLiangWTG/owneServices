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
	[TestedType(typeof(MultipleEmailTemplatesRegistryEditor))]
	public class MultipleEmailTemplatesRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MultipleEmailTemplatesRegistryItem("", null, null, null, RegistryStorageFlags.System, typeof(DocSupportIncident));
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new MultipleEmailTemplatesRegistryEditor(new MultipleEmailTemplatesRegistryDataType(typeof(DocSupportIncident)), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MultipleEmailTemplatesControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			CodeDescriptionEmailTemplateCollection collection = new CodeDescriptionEmailTemplateCollection(typeof(DocSupportIncident));
			CodeDescriptionEmailTemplate template1 = collection.AddNew();
			template1.Code = "AAA";
			template1.Description = (NoResString)"Template One";
			template1.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject one", "Email body template one");
			CodeDescriptionEmailTemplate template2 = collection.AddNew();
			template2.Code = "BBB";
			template2.Description = (NoResString)"Template Two";
			template2.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject two", "Email body template two");
			return new object[] { collection };
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
			return !((MultipleEmailTemplatesControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
