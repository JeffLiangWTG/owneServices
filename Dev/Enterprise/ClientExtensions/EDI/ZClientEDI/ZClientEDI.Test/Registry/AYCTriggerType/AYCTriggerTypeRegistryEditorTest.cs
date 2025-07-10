using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(AYCTriggerTypeRegistryEditor))]
	public class AYCTriggerTypeRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AYCTriggerTypeRegistryItem("AYCTriggerType", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new AYCTriggerTypeRegistryEditor(new AYCTriggerTypeDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AYCTriggerTypeSettingsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var item = new AYCTriggerTypeSettings();
			using (item.GetValidationSuspender())
			{
				item.PrimaryChargeCode = "PRIMARY";
				item.SecondaryChargeCode = "SECOND";
			}

			return new[] { item };
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
			return !((AYCTriggerTypeSettingsControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
