using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(AWSPrivateCARegistryItemEditor))]
	public class AWSPrivateCARegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new AWSPrivateCARegistryItemEditor(new AWSPrivateCARegistryDataType(),
				new ZArchitecture.Environment.FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AWSPrivateCAControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AWSPrivateCAControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AWSPrivateCARegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new AWSPrivateCACollection()
			{
				new AWSPrivateCA()
				{
					IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust,
					Arn = "pc:ca:arn",
					IsEnabled = ZBool.True,
					AccessKey = "AccessKeyTest01",
					SecretKey = "SecretKeyTest01"
				}
			} };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
