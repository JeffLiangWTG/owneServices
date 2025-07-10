using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class OrderedListRegistryItemEditor : RegistryItemEditor
	{
		public OrderedListRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new OrderedListControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((OrderedListControl)editorPane).Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((OrderedListControl)editorPane).Value = (string)value;
		}
	}
}
