using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ContactSalutationRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public ContactSalutationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.fallbackLevel = fallbackLevel;
			this.factory = factory;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ContactSalutationControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ContactSalutationControl)editorPane).Data;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var clone = ((ContactSalutationCollection)value)?.Clone(fallbackLevel, factory);

			((ContactSalutationControl)editorPane).Data = clone ?? new ContactSalutationCollection();
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((ContactSalutationControl)editorPane).ReadOnly = !enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
