using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ParameterizedStringRegistryItemEditor : RegistryItemEditor
	{
		public ParameterizedStringRegistryItemEditor(IRegistryItem item)
			: base(item.DataType)
		{
			this.registryItem = (ParameterizedStringRegistryItem)item;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ParameterizedStringControl(registryItem);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return registryItem.Deserialise(((ParameterizedStringControl)editorPane).valueTextBox.Text);
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((ParameterizedStringControl)editorPane).valueTextBox.Text = ((ResourceString)value).ToStringWithParameters(Res.DefaultLanguage);
		}

		public override void SetEditorPaneLayout(Control editorPane, int width, int height)
		{
			editorPane.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			ControlDpiScalingHelper.SetHeight(ref editorPane, height, false);
			ControlDpiScalingHelper.SetWidth(ref editorPane, width, false);
		}

		readonly ParameterizedStringRegistryItem registryItem;
	}
}
