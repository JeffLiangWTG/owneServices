using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchCodeMappingRegistryBusinessObject))]
	public class BranchCodeMappingRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<BranchCodeMappingRegistryBusinessObject>
	{
		public void TestProperties()
		{
			AssertNotNull(BizObj.BranchCodes);
			AssertEquals(0, BizObj.BranchCodes.Count);

			GlbBranch branchCode = BizObj.BranchCodes.AddNew();
			branchCode.GB_Code = "BLA";

			AssertEquals(1, BizObj.BranchCodes.Count);
			AssertEquals("BLA", BizObj.BranchCodes[0].GB_Code);

			BranchCodeMappingRegistryBusinessObject bizObj = (BranchCodeMappingRegistryBusinessObject)GetNewBusinessObject();
			bizObj.CodePK = ZGuid.Empty;
			bizObj.ExternalCode = ZString.Empty;

			AssertEquals("Code PK should have errors", true, bizObj.CodePKInfo.HasErrors());
			AssertEquals("ExternalCode should have errors", true, bizObj.ExternalCodeInfo.HasErrors());
		}

		#region Implementation
		protected override BranchCodeMappingRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override BranchCodeMappingRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			BizObj.CodePK = GetBranchCodeToTest().PK;
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
			return new BranchCodeMappingRegistryBusinessObject(Factory);
		}

		GlbBranch GetBranchCodeToTest()
		{
			return Factory.Load<GlbBranch>(new ZGuid("FDD429D2-648C-4895-8F9F-06E90DED2BE5"));  // GB_Code = "SYD"
		}
		#endregion
	}
}
