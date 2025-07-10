using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BiReportCredentialRegistryItemEditor))]
	sealed class BiReportCredentialRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[ExpectNoExceptions]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testvalue"))
			{
				RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential());
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BiReportCredentialRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BiReportCredentialControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BiReportCredentialControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BiReportCredentialRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BiReportCredential());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new BiReportCredential() };
		}
	}
}
