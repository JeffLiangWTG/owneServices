using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(NotInterfaceConnectorLicencedRegistryItemEditor))]
	sealed class NotInterfaceConnectorLicensedRegistryItemEditorTestCase : RegistryItemEditorTestCase
	{
		public void TestNewWinFormsEditorPaneCore()
		{
			using (var control = new NotInterfaceConnectorLicensedRegistryItemEditorTest().NewWinFormsEditorPaneCoreTest())
			{
				AssertType(typeof(ZLabel), control);
				var label = (ZLabel)control;
				AssertEquals("This module is no longer available in CW1. This functionality has been superseded by the eAdaptor web service interface. Please contact your sales rep.", label.Text);
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new NotInterfaceConnectorLicencedRegistryItemEditor();
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ZLabel);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LicencedStringRegistryItem(() => false, "", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { "Bla" };
		}
	}
}
