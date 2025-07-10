using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

partial class UCC6TemporaryStoragePackedItemDetailsControl : ZUserControl
{
	public UCC6TemporaryStoragePackedItemDetailsControl()
	{
		InitializeComponent();
		// There is an issue for PackedItem Control to make fields readonly, will be fixed in another WI
		RegistrationNoTextBox.ReadOnly = true;
		ReleaseDateEdit.ReadOnly = true;
	}
}
