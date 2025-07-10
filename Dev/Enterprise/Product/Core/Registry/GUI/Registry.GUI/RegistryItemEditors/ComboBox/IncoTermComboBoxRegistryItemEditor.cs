using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class IncoTermComboBoxRegistryItemEditor : ComboBoxRegistryItemEditor
	{
		public IncoTermComboBoxRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType, editorInfo)
		{
		}

		protected override DropEditBusinessObject GetNewDropEditBusinessObject()
		{
			return new IncotermsDropEditBusinessObject(editorInfo.LookUpList);
		}
	}
}
