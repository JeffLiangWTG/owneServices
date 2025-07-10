using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class RateValidityRegistryItemEditor : RegistryItemEditor
	{
		public RateValidityRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			RateValidityControl result = new RateValidityControl();
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((RateValidityControl)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((RateValidityControl)editorPane).FieldValue = (int)value;
		}
	}
}
