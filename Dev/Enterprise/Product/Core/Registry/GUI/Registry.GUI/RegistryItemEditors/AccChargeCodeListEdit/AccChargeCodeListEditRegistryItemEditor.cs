using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AccChargeCodeListEditRegistryItemEditor : RegistryItemEditor
	{
		public AccChargeCodeListEditRegistryItemEditor(IRegistryEditorInfo editorInfo, FallbackLevel fallback, BusinessObjectFactory factory) : base(null)
		{
			this.EditorInfo = (AccChargeCodeListRegistryEditorInfo)editorInfo;
			this.Fallback = fallback;
			this.Factory = factory;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new AccChargeCodeListEditContainer(EditorInfo.Filter, Factory, Fallback.CompanyPK(false));
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((AccChargeCodeListEditContainer)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((AccChargeCodeListEditContainer)editorPane).FieldValue = (string)value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly FallbackLevel Fallback;
		readonly BusinessObjectFactory Factory;
		readonly AccChargeCodeListRegistryEditorInfo EditorInfo;
	}
}
