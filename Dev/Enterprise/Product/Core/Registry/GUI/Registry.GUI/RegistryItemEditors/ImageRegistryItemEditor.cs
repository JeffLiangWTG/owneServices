using System.Drawing;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class ImageRegistryItemEditor : RegistryItemEditor
	{
		public ImageRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var control = new ImageSelectionControl();
			control.ImageObjectChangedByUser += (sender, e) => NotifyChanges((Control)sender);

			return control;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ImageSelectionControl)editorPane).Image;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((ImageSelectionControl)editorPane).Image = (Image)value;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((ImageSelectionControl)editorPane).Enabled = enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
