using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public class WebEDocsDownloadRegistryItemEditor : RegistryItemEditor
	{
		public WebEDocsDownloadRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		#region Implementation

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new WebEDocsDownloadRegistryControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((WebEDocsDownloadRegistryControl)editorPane).ModuleDocTypeList;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((WebEDocsDownloadRegistryControl)editorPane).ModuleDocTypeList = ((WebEDocsDownloadEntryDictionary)value);
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		#endregion
	}
}
