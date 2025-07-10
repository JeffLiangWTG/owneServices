using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(DepartmentCodeMappingRegistryItemEditor))]
	public class DepartmentCodeMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DepartmentCodeMappingRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override object[] GetValidRegistryValues()
		{
			DepartmentCodeMappingRegistryBusinessObjectCollection collection = new DepartmentCodeMappingRegistryBusinessObjectCollection();
			DepartmentCodeMappingRegistryBusinessObject element = collection.AddNew();
			element.CodePK = GetChargeCodeToTest().PK;
			element.ExternalCode = "BOB";
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((DepartmentCodeMappingRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DepartmentCodeMappingRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DepartmentCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		GlbDepartment GetChargeCodeToTest()
		{
			return Factory.LoadTop1<GlbDepartment>(new ZQuery());//new ZGuid("86BB1C22-0865-4685-996E-D56CBD136491"));  // GE_Code = "BRN"
		}
	}
}
