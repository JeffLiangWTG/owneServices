using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ZAddressRegistryItemEditorForTest : ZAddressRegistryItemEditor
	{
		public ZAddressRegistryItemEditorForTest(IRegistryDataType dataType) : base(dataType)
		{
		}

		public Type GetEditorPaneType()
		{
			return typeof(MyZAddressControl);
		}

		public void SetValueZGuid(Control editorPane, ZGuid value)
		{
			((MyZAddressControl)editorPane).DataSource.SelectedAddress = value;
		}
	}
}
