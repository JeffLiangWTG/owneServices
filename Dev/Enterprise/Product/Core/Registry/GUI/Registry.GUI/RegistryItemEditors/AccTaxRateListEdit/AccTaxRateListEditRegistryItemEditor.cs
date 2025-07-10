using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AccTaxRateListEditRegistryItemEditor : RegistryItemEditor
	{
		public AccTaxRateListEditRegistryItemEditor(IRegistryEditorInfo editorInfo, FallbackLevel fallback, BusinessObjectFactory factory) : base(null)
		{
			this.EditorInfo = (AccTaxRateListRegistryEditorInfo)editorInfo;
			this.Fallback = fallback;
			this.Factory = factory;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new AccTaxRateListEditContainer(EditorInfo.Filter, Factory, Fallback.CompanyPK(false));
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((AccTaxRateListEditContainer)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((AccTaxRateListEditContainer)editorPane).FieldValue = (string)value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly FallbackLevel Fallback;
		readonly BusinessObjectFactory Factory;
		readonly AccTaxRateListRegistryEditorInfo EditorInfo;
	}
}
