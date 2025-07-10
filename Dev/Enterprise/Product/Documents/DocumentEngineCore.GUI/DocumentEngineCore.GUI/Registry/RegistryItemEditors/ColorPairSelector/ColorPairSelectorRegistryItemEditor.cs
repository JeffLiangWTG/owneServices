using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public sealed class ColorPairSelectorRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public ColorPairSelectorRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.factory = factory;
			fallbackLevel = fallback;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ColorPairSelectorControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ColorPairSelectorControl)editorPane).ColorPair;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var clone = ((ColorPairSelector)value).Clone(fallbackLevel, factory);

			((ColorPairSelectorControl)editorPane).ColorPair = (ColorPairSelector)clone;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
