using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

static class AEManifestValidationHelper
{
	public static void AssertOrgHasContactInfo(OrgAddress orgAddress, ZPropertyInfo targetInfo, Action invokeValidation)
	{
		const string errorMsg = "Organization requires a Phone Number, Email Address or Website URL";

		targetInfo.Value = orgAddress.PK;
		TestCaseWithFactory.AssertHasMessageError("Org without contact info", targetInfo, errorMsg);

		orgAddress.OA_Phone = "+1 23 45 67 890";
		invokeValidation();
		TestCaseWithFactory.AssertNoMessageError("Org has phone number", targetInfo, errorMsg);

		orgAddress.OA_Phone = ZString.Empty;
		orgAddress.OA_Email = "abc@xyz.com";
		invokeValidation();
		TestCaseWithFactory.AssertNoMessageError("Org has email", targetInfo, errorMsg);

		orgAddress.OA_Email = ZString.Empty;
		var orgWebUrl = orgAddress.Header.MainWebURL;
		orgWebUrl.PU_URL = "www.dummyurl.com";
		invokeValidation();
		TestCaseWithFactory.AssertNoMessageError("Org has website url", targetInfo, errorMsg);
	}
}
