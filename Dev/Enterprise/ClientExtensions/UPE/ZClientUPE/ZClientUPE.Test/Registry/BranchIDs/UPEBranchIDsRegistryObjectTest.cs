using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEBranchIDsRegistryObject))]
	public class UPEBranchIDsRegistryObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Validation
		public void TestValidateBranch()
		{
			AssertNoErrors("Precondition: Branch should not have errors.", BizObj.FirstArrivalPortInfo);
			BizObj.FirstArrivalPort = GlbCompany.CurrentCompany.PK;
			AssertHasError(BizObj.FirstArrivalPortInfo, "Enter a valid selection.");
			BizObj.FirstArrivalPort = GlbBranch.CurrentBranch.HomePort.PK;
			AssertNoError(BizObj.FirstArrivalPortInfo, "Enter a valid selection.");
			BizObj.FirstArrivalPort = new ZGuid();
			AssertNoError(BizObj.FirstArrivalPortInfo, "Enter a valid selection.");
		}

		public void TestValidateBuildingID()
		{
			AssertNoErrors("Precondition: BuildingID should not have errors.", BizObj.BuildingIDInfo);
			BizObj.BuildingID = "SSSFFG";
			AssertHasError(BizObj.BuildingIDInfo, "Building ID should be 7 chars length");
			BizObj.BuildingID = ZString.Empty;
			AssertHasError(BizObj.BuildingIDInfo, "Please enter a value.");
			BizObj.BuildingID = "AUAUSYD";
			AssertNoErrors(BizObj.BuildingIDInfo);
		}

		#endregion
		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			UPEBranchIDsRegistryObject result = new UPEBranchIDsRegistryObject();
			result.FirstArrivalPort = GlbBranch.CurrentBranch.PK;
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected new UPEBranchIDsRegistryObject BizObj
		{
			get
			{
				return (UPEBranchIDsRegistryObject)base.BizObj;
			}
		}
		#endregion
	}
}
