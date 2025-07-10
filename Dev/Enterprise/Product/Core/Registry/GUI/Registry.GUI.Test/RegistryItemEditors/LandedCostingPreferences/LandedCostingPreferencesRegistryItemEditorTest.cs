using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(LandedCostingPreferencesRegistryItemEditor))]
	sealed class LandedCostingPreferencesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new LandedCostingPreferencesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((LandedCostingPreferencesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LandedCostingPreferencesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LandedCostingPreferencesRegistryItem("", null, null, null, RegistryStorageFlags.System, new LandedCostingGroupCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();

			LandedCostingGroup costingGroup = collection.AddNew();
			costingGroup.GroupID = 1;
			costingGroup.GroupName = "Group 1";
			costingGroup.CostDistributionCode = "AWV";

			ChargeGroupAndChargeCode charge = costingGroup.Charges.AddNew();
			charge.ChargeCodePK = new BusinessObjectFactory().LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
