using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class ScheduleControlRegistryItemEditor : RegistryItemEditor
	{
		public ScheduleControlRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			OScheduleControl result = new OScheduleControl();
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((OScheduleControl)editorPane).FullWeekText;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((OScheduleControl)editorPane).FullWeekText = (string)value;
		}
	}
}
