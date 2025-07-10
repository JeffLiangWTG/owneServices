using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class eAdaptorNextOutboundRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public eAdaptorNextOutboundRegistryItemEditor(IRegistryDataType dataType)
			: this(dataType, null, null)
		{
		}

		public eAdaptorNextOutboundRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.factory = factory;
			this.fallbackLevel = fallback;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((eAdaptorNextOutboundRegistryControl)editorPane).Value;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new eAdaptorNextOutboundRegistryControl();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var clone = ((eAdaptorNextOutboundConfig)value).Clone(fallbackLevel, factory);
			((eAdaptorNextOutboundRegistryControl)editorPane).Value = (eAdaptorNextOutboundConfig)clone;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((eAdaptorNextOutboundRegistryControl)editorPane).ReadOnly = !enabled;
		}
	}
}
