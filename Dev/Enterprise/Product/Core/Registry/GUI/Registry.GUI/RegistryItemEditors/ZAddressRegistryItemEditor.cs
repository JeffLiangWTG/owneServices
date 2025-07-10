using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class ZAddressRegistryItemEditor : RegistryItemEditor
	{
		public ZAddressRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			ZAddressBusinessObject myBusinessObject = new ZAddressBusinessObject();
			MyZAddressControl result = new MyZAddressControl(myBusinessObject);
			result.BindToOrgList = "OrgHeaders";
			result.SetDataBinding(myBusinessObject, "SelectedAddress");
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			ZGuid value = ((MyZAddressControl)editorPane).DataSource.SelectedAddress;
			Guid result = (value.IsEmpty || !value.IsValid) ? Guid.Empty : value.ToGuid();
			return result;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((MyZAddressControl)editorPane).DataSource.SelectedAddress = (Guid)value;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((MyZAddressControl)editorPane).Enabled = enabled;
			((MyZAddressControl)editorPane).DataSource.ReadOnly = !enabled;
		}

		public override string GetCustomValidation(Control editorPane)
		{
			return ((MyZAddressControl)editorPane).DataSource.ErrorMessage;
		}
	}
}
