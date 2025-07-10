using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory.GUI.Registry
{
	public class ADRegistryControlItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public ADRegistryControlItemEditor(IRegistryDataType dataType)
			: this(dataType, null, null)
		{
		}

		public ADRegistryControlItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.factory = factory;
			this.fallbackLevel = fallback;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ADRegistryControl)editorPane).Value;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ADRegistryControl();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var clone = ((ADConfig)value).Clone(fallbackLevel, factory);
			((ADRegistryControl)editorPane).Value = (ADConfig)clone;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((ADRegistryControl)editorPane).ReadOnly = !enabled;
		}
	}
}
