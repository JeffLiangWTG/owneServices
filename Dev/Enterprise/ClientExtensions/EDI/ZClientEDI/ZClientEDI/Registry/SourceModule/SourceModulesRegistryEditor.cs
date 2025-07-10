using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class SourceModulesRegistryEditor : RegistryItemEditor
	{
		public SourceModulesRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new SourceModulesControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((IDataBoundControl)editorPane).DataSource;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var sourceModuleCollection = (SourceModuleCollection)value ?? new SourceModuleCollection();

			var containerControl = (ZUserControl)editorPane;
			containerControl.SetDataBinding(sourceModuleCollection.GetCopy(), null);
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
