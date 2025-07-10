using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DepotAddressColorSoundRegistryItemEditor))]
	sealed class DepotAddressColorSoundRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DepotAddressColorSoundRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DepotAddressColorSoundControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DepotAddressColorSoundControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DepotAddressColorSoundRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			DepotAddressColorSoundCollection collection = new DepotAddressColorSoundCollection(Factory);

			DepotAddressColorSound depotAddressColorSound = collection.AddNew();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsPackDepot = true;

			depotAddressColorSound.Organisation = org.PK;
			depotAddressColorSound.Address = org.MainAddress.PK;
			depotAddressColorSound.Color = "123123123";
			depotAddressColorSound.MP3FileName = "Test.mp3";
			depotAddressColorSound.MP3FileContent = ZBlob.FromAscii("1234567890");

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
