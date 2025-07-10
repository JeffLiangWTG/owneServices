using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(DepartmentCodeMappingRegistryBusinessObject))]
	public class DepartmentCodeMappingRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<DepartmentCodeMappingRegistryBusinessObject>
	{
		public void TestProperties()
		{
			AssertNotNull(BizObj.DepartmentCodes);

			GlbDepartment departmentCode = BizObj.DepartmentCodes.AddNew();
			departmentCode.GE_Code = "BLA";
			BizObj.DepartmentCodes.AdditionalFilter = new ZQuery(GlbDepartmentSchema.GE_Code, "BLA");
			AssertEquals("BLA", BizObj.DepartmentCodes[0].GE_Code);

			DepartmentCodeMappingRegistryBusinessObject bizObj = (DepartmentCodeMappingRegistryBusinessObject)GetNewBusinessObject();
			bizObj.CodePK = ZGuid.Empty;
			bizObj.ExternalCode = ZString.Empty;

			AssertEquals("Code PK should have errors", true, bizObj.CodePKInfo.HasErrors());
			AssertEquals("ExternalCode should have errors", true, bizObj.ExternalCodeInfo.HasErrors());
		}

		#region Implementation
		protected override DepartmentCodeMappingRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override DepartmentCodeMappingRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			BizObj.CodePK = GetDepartmentCodeToTest().PK;
			BizObj.ExternalCode = "BOB";
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DepartmentCodeMappingRegistryBusinessObject(Factory);
		}

		GlbDepartment GetDepartmentCodeToTest()
		{
			return Factory.Load<GlbDepartment>(new ZGuid("86BB1C22-0865-4685-996E-D56CBD136491"));  // GE_Code = "BRN"
		}
		#endregion
	}
}
