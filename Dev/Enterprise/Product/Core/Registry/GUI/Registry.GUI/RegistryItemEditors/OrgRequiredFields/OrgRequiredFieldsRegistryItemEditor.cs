using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class OrgRequiredFieldsRegistryItemEditor : RegistryItemEditor
	{
		readonly bool isDebtorRequiredFields;

		public OrgRequiredFieldsRegistryItemEditor(bool isDebtorRequiredFields, IRegistryDataType dataType) : base(dataType)
		{
			this.isDebtorRequiredFields = isDebtorRequiredFields;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var result = new OrgRequiredFieldsControl(isDebtorRequiredFields);
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((OrgRequiredFieldsControl)editorPane).GetFields().GetRegistryValue();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((OrgRequiredFieldsControl)editorPane).SetFields(new OrgRequiredFields((byte[])value));
		}
	}
}
