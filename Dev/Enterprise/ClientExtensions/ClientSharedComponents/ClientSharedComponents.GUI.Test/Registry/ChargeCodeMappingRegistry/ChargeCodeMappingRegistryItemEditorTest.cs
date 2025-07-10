using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ChargeCodeMappingRegistryItemEditor))]
	public class ChargeCodeMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ChargeCodeMappingRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override object[] GetValidRegistryValues()
		{
			ChargeCodeMappingRegistryBusinessObjectCollection collection = new ChargeCodeMappingRegistryBusinessObjectCollection();
			ChargeCodeMappingRegistryBusinessObject element = collection.AddNew();
			element.CodePK = GetChargeCodeToTest().PK;
			element.ExternalCode = "BOB";
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ChargeCodeMappingRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ChargeCodeMappingRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ChargeCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		AccChargeCode GetChargeCodeToTest()
		{
			return Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));//new ZGuid("8319278C-E149-4895-BC52-114E69E069D9"));  // AC_Code = "FRT"
		}
	}
}
