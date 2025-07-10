using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(LocationsChargesRegistryItemEditor))]
	sealed class LocationsChargesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new LocationsChargesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((LocationsChargesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LocationsChargesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LocationsChargesRegistryItem("", null, null, null, RegistryStorageFlags.System, new LocationsChargesCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			LocationsChargesCollection collection = new LocationsChargesCollection();

			LocationsChargesGroup chargesGroup = collection.AddNew();
			chargesGroup.Location = "USNYC";
			ChargeCodeGroup charge = chargesGroup.Charges.AddNew();
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
