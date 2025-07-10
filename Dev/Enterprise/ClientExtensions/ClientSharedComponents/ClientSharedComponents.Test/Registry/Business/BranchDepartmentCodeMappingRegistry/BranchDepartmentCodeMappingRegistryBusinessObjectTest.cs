using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchDepartmentCodeMappingRegistryBusinessObject))]
	public class BranchDepartmentCodeMappingRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<BranchDepartmentCodeMappingRegistryBusinessObject>
	{
		public void TestProperties()
		{
			AssertNotNull(BizObj.BranchCodes);
			AssertEquals(0, BizObj.BranchCodes.Count);
			AssertNotNull(BizObj.DepartmentCodes);

			GlbBranch branchCode = BizObj.BranchCodes.AddNew();
			branchCode.GB_Code = "BLA";
			GlbDepartment deptCode = BizObj.DepartmentCodes.AddNew();
			deptCode.GE_Code = "DAH";
			BizObj.DepartmentCodes.AdditionalFilter = new ZQuery(GlbDepartmentSchema.GE_Code, "DAH");

			AssertEquals(1, BizObj.BranchCodes.Count);
			AssertEquals("BLA", BizObj.BranchCodes[0].GB_Code);
			AssertEquals(1, BizObj.DepartmentCodes.Count);
			AssertEquals("DAH", BizObj.DepartmentCodes[0].GE_Code);

			BranchDepartmentCodeMappingRegistryBusinessObject bizObj = (BranchDepartmentCodeMappingRegistryBusinessObject)GetNewBusinessObject();
			bizObj.BranchCodePK = ZGuid.Empty;
			bizObj.DepartmentCodePK = ZGuid.Empty;
			bizObj.ProfitCentre = ZString.Empty;
			bizObj.NominalDepartment = ZString.Empty;

			AssertEquals("Branch code PK should have errors", true, bizObj.BranchCodePKInfo.HasErrors());
			AssertEquals("Department code PK should have errors", true, bizObj.DepartmentCodePKInfo.HasErrors());
			AssertEquals("Profit centre should have errors", true, bizObj.ProfitCentreInfo.HasErrors());
			AssertEquals("Nominal department should have errors", true, bizObj.NominalDepartmentInfo.HasErrors());
		}

		#region Implementation
		protected override BranchDepartmentCodeMappingRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override BranchDepartmentCodeMappingRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			BizObj.BranchCodePK = GetBranchCodeToTest().PK;
			BizObj.DepartmentCodePK = GetDepartmentCodeToTest().PK;
			BizObj.ProfitCentre = "BOB";
			BizObj.NominalDepartment = "BAZ";
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
			return new BranchDepartmentCodeMappingRegistryBusinessObject(Factory);
		}

		GlbBranch GetBranchCodeToTest()
		{
			return TestHelper.FindOrCreateBranch("SYD");
		}

		GlbDepartment GetDepartmentCodeToTest()
		{
			return TestHelper.FindOrCreateDepartment("BRN");
		}
		#endregion

		SharedTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new SharedTestHelper(Factory)); }
		}
		SharedTestHelper testHelper;
	}
}
