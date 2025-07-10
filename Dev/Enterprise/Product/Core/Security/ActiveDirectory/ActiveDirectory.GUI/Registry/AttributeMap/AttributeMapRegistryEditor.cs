using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI.Registry
{
	public class AttributeMapRegistryEditor : RegistryItemEditor
	{
		public AttributeMapRegistryEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore() => new AttributeMapControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		protected override object GetValueFromEditorPaneCore(Control editorPane) => ((AttributeMapControl)editorPane).Value;

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var propertyMap = value as AttributeMap;
			if (propertyMap != null)
			{
				((AttributeMapControl)editorPane).Value = (AttributeMap)propertyMap.Clone(null, null);
			}
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled) => ((AttributeMapControl)editorPane).IsReadOnly = !enabled;
	}
}
