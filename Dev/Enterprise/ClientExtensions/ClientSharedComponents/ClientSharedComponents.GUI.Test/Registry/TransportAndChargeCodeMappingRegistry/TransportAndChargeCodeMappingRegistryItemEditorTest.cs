using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(TransportAndChargeCodeMappingRegistryItemEditor))]
	public class TransportAndChargeCodeMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new TransportAndChargeCodeMappingRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override object[] GetValidRegistryValues()
		{
			TransportAndChargeCodeMappingRegistryBusinessObjectCollection collection = new TransportAndChargeCodeMappingRegistryBusinessObjectCollection();
			TransportAndChargeCodeMappingRegistryBusinessObject element = collection.AddNew();
			element.TransportModeCode = "SEA";
			element.NominalCostCode = "BOB";
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((TransportAndChargeCodeMappingRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TransportAndChargeCodeMappingRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TransportAndChargeCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
