using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(SendOrganizationDataToCertCaptureRegistryEditor))]
	public class SendOrganizationDataToCertCaptureRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SendOrganizationDataToCertCaptureRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new SendOrganizationDataToCertCapture());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new SendOrganizationDataToCertCaptureRegistryEditor(new SendOrganizationDataToCertCaptureDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SendOrganizationDataToCertCaptureControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new SendOrganizationDataToCertCapture() };
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
			return !((SendOrganizationDataToCertCaptureControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
