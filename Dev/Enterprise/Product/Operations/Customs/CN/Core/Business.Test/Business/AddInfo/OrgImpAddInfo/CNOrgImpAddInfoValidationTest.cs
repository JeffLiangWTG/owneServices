using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNOrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZO_MessageSubType()
		{
			var orgImpAddInfo = CNOrgImpAddInfo.Get(Factory.New<OrgHeader>());
			ValidationTestHelper.AssertInvalidCodeMessageError(orgImpAddInfo.ZO_MessageSubTypeInfo, new ZString[] { "XXX" }, new ZString[] { "REC", "CUS" });
			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(orgImpAddInfo.ZO_MessageSubTypeInfo, new ZString[] { "XXX" }, new ZString[] { "REC", "CUS", "BTH" });
			}
		}

		public void TestCheckZO_IntelligentDeclarationType()
		{
			var orgImpAddInfo = CNOrgImpAddInfo.Get(Factory.New<OrgHeader>());
			ValidationTestHelper.AssertInvalidCodeMessageError(orgImpAddInfo.ZO_IntelligentDeclarationTypeInfo, new ZString[] { "X" }, new ZString[] { "0", "1", "2" });
		}
	}
}
