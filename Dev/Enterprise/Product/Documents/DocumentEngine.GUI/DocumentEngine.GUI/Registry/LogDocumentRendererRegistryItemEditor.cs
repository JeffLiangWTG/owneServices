namespace Enterprise.DocumentEngine.GUI.Registry
{
	using System.Windows.Forms;
	using CargoWise.EntityFramework;
	using Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer;
	using Enterprise.Integration;
	using Enterprise.Registry.GUI;
	using Enterprise.ZArchitecture.Environment;

	public class LogDocumentRendererRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public LogDocumentRendererRegistryItemEditor(LogDocumentRendererRegistryDataType dataType)
			: this(dataType, null, null)
		{
		}

		public LogDocumentRendererRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.factory = factory;
			this.fallbackLevel = fallback;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new LogDocumentRendererRegistryControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			var view = (LogDocumentRendererRegistryControl)editorPane;
			return view.CurrentDataItem;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var view = (LogDocumentRendererRegistryControl)editorPane;
			var clone = ((LogDocumentRendererRegistry)value).Clone(fallbackLevel, factory);

			view.SetDataBinding(clone, string.Empty);
		}
	}
}
