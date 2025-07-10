using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class QuoteValidityRegistryItemEditor : RegistryItemEditor
	{
		public QuoteValidityRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			QuoteValidityControl result = new QuoteValidityControl();
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((QuoteValidityControl)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((QuoteValidityControl)editorPane).FieldValue = (int)value;
		}
	}
}
