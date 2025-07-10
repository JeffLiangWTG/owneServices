using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class CusSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference()
		{
			const string error = "You have not entered an Identification Number.";
			const string errorEor = "The selected Organization (ABCZXY) does not contain an EOR Customs Code.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABCZXY";

			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;

			var propertyInfo = cusSupplyChainActorReference.CFR_ReferenceInfo;
			Assert(propertyInfo.ReadOnly);
			AssertHasError(propertyInfo, errorEor);

			cusSupplyChainActorReference.CFR_Reference = "ABCZXY123";
			AssertNoError(propertyInfo, errorEor);

			cusSupplyChainActorReference.CFR_OA_Owner = ZGuid.Empty;
			cusSupplyChainActorReference.CFR_Reference = ZString.Empty;
			Assert(!propertyInfo.ReadOnly);
			AssertHasError(propertyInfo, error);

			cusSupplyChainActorReference.CFR_Reference = "ABCZXY123";
			AssertNoError(propertyInfo, error);
		}
	}
}
