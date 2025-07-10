using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(CcsukNonstandardPimaRegistryItemEditor))]
	public class CcsukNonstandardPimaRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CcsukNonstandardPimaRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((CcsukNonstandardPimaControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CcsukNonstandardPimaControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CcsukNonstandardPimaSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System, new CcsukNonstandardPimaSettingCollection().GetDefaultValues());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CcsukNonstandardPimaSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			collection.Add(new CcsukNonstandardPimaSetting("A", "BTH", "C", Factory));
			collection.Add(new CcsukNonstandardPimaSetting("A", "FRN", "C", Factory));
			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var collection1 = (CcsukNonstandardPimaSettingCollection)setValue;
			var collection2 = (CcsukNonstandardPimaSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(collection1[i].AirportAndShed, collection2[i].AirportAndShed);
				AssertEquals(collection1[i].MessageType, collection2[i].MessageType);
				AssertEquals(collection1[i].TypeBPima, collection2[i].TypeBPima);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}

	[TestedType(typeof(CcsukNonstandardPimaControl))]
	public class CcsukNonstandardPimaControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CcsukNonstandardPimaSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CcsukNonstandardPimaControl)control).AddressesGrid.ReadOnly;
		}
	}
}
