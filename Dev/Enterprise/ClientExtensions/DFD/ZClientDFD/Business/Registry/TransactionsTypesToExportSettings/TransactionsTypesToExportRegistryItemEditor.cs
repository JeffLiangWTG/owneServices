using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DFD.Registry
{
	internal class TransactionsTypesToExportRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public TransactionsTypesToExportRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new TransactionsTypesToExportControl();
		}
	}
}
