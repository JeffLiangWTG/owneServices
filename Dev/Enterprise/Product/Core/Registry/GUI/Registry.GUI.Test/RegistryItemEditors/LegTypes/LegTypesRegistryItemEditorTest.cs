using System;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(LegTypesRegistryItemEditor))]
	sealed class LegTypesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new LegTypesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((LegTypesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LegTypesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LegTypeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new LegTypeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			LegTypeCollection collection = new LegTypeCollection();
			LegType legType = collection.AddNew();
			legType.Code = "ABC";
			legType.Description = (NoResString)"Desc";
			legType.PickupFromOrg = "CTO";
			legType.WaitPointOrg = "CFS";
			legType.DeliverToOrg = "CNE";
			legType.MovementType = Constants.CartageDirection.Destination;
			legType.Containerised = "CNT";
			legType.EquipmentGroup = "WUP";
			legType.IsSystemDefined = true;

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			LegTypeCollection collection1 = (LegTypeCollection)setValue;
			LegTypeCollection collection2 = (LegTypeCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Code", i.ToString()), collection1[i].Code, collection2[i].Code);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Description", i.ToString()), collection1[i].Description, collection2[i].Description);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PickupFromOrg", i.ToString()), collection1[i].PickupFromOrg, collection2[i].PickupFromOrg);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].WaitPointOrg", i.ToString()), collection1[i].WaitPointOrg, collection2[i].WaitPointOrg);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].DeliverToOrg", i.ToString()), collection1[i].DeliverToOrg, collection2[i].DeliverToOrg);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].MovementType", i.ToString()), collection1[i].MovementType, collection2[i].MovementType);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Containerised", i.ToString()), collection1[i].Containerised, collection2[i].Containerised);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].EquipmentGroup", i.ToString()), collection1[i].EquipmentGroup, collection2[i].EquipmentGroup);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].IsSystemDefined", i.ToString()), collection1[i].IsSystemDefined, collection2[i].IsSystemDefined);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
