using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(EInvoicingCertificateExpiryNotificationGroupRegistryItemEditor))]
	class EInvoicingCertificateExpiryNotificationGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new EInvoicingCertificateExpiryNotificationGroupRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EInvoicingCertificateExpiryNotificationGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EInvoicingCertificateExpiryNotificationGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RegistryItemImpl("", null, null, null, new EInvoicingCertificateExpiryNotificationGroupRegistryDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company, new EInvoicingCertificateExpiryNotificationGroup());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new EInvoicingCertificateExpiryNotificationGroup() };
		}
	}
}