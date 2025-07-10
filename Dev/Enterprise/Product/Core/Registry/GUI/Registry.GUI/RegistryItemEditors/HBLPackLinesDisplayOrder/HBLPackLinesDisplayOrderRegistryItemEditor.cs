using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class HBLPackLinesDisplayOrderRegistryItemEditor : ComboBoxRegistryItemEditor
	{
		public HBLPackLinesDisplayOrderRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType, editorInfo)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var control = new HBLPackLinesDisplayOrderControl();
			control.SetDataBinding(GetNewDropEditBusinessObject(), string.Empty);
			return control;
		}

		protected override DropEditBusinessObject GetNewDropEditBusinessObject()
		{
			return new HBLPackLinesDisplayOrderDropEditBussinessObject(editorInfo.LookUpList);
		}
	}
}
