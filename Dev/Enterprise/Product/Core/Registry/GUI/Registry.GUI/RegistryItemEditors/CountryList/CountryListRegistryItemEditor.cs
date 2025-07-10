using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.GUI
{
	public class CountryListRegistryItemEditor : RegistryItemEditor
	{
		public CountryListRegistryItemEditor(BusinessObjectFactory factory)
			: base(null)
		{
			this.factory = factory;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new CountryListControl(factory);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((CountryListControl)editorPane).CountryPKs;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((CountryListControl)editorPane).CountryPKs = (Guid[])value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly BusinessObjectFactory factory;
	}
}
