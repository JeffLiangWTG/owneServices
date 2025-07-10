using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.Registry
{
	public class DigitalSignatureRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public DigitalSignatureRegistryItemEditor(DigitalSignatureRegistryDataType dataType)
			: this(dataType, null, null)
		{
		}

		public DigitalSignatureRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.factory = factory;
			fallbackLevel = fallback;
		}

		protected override Control NewWinFormsEditorPaneCore() => new DigitalSignatureRegistryControl();

		protected override object GetValueFromEditorPaneCore(Control editorPane) => ((DigitalSignatureRegistryControl)editorPane).CurrentDataItem;

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var view = (DigitalSignatureRegistryControl)editorPane;
			var clone = ((DigitalSignatureRegistry)value).Clone(fallbackLevel, factory);

			view.SetDataBinding(clone, string.Empty);
		}
	}
}
