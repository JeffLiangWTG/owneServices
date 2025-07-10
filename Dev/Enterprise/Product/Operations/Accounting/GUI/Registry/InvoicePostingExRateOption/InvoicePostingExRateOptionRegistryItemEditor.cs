using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.GUI
{
	public class InvoicePostingExRateOptionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public InvoicePostingExRateOptionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
			: base(dataType, currentFallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new InvoicePostingExRateOptionControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
