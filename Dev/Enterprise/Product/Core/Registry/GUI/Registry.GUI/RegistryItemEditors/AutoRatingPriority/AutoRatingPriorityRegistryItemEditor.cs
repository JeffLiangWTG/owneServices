using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class AutoRatingPriorityRegistryItemEditor : RegistryItemEditor
	{
		public AutoRatingPriorityRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			AutoRatingPriorityControl result = new AutoRatingPriorityControl();
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((AutoRatingPriorityControl)editorPane).Priorities;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((AutoRatingPriorityControl)editorPane).Priorities = (string)value;
		}
	}
}
