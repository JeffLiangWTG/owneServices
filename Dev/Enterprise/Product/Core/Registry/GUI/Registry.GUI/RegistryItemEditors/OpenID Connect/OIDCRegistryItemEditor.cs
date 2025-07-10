using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class OIDCRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public OIDCRegistryItemEditor(IRegistryDataType dataType)
			: this(dataType, null, null)
		{
		}

		public OIDCRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.factory = factory;
			this.fallbackLevel = fallback;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((OIDCRegistryControl)editorPane).Value;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new OIDCRegistryControl((DataType as OIDCConfigRegistryDataType).IsWinzorConfig);
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var clone = ((OIDCConfig)value).Clone(fallbackLevel, factory);
			((OIDCRegistryControl)editorPane).Value = (OIDCConfig)clone;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((OIDCRegistryControl)editorPane).ReadOnly = !enabled;
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
