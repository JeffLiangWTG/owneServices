using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AutoRatingRequiredFieldsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AutoRatingRequiredFieldsRegistryItemEditor(AutoRatingRequiredFieldsRegistryEditorInfo editorInfo, IRegistryDataType dataType)
			: base(dataType, null, null)
		{
			this.editorInfo = editorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return (editorInfo.ShowIncoterm) ? new AutoRatingRequiredFieldsWithIncotermControl() : new AutoRatingRequiredFieldsControl();
		}

		readonly AutoRatingRequiredFieldsRegistryEditorInfo editorInfo;
	}
}
