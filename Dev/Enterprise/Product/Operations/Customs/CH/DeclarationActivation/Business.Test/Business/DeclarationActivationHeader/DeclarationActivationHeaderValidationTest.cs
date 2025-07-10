using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Business.Testing;

[TestedType(typeof(DeclarationActivationHeaderValidation))]
sealed class DeclarationActivationHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCXH_GS_NKCustomsAgent()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationHeader.CXH_GS_NKCustomsAgentInfo, "XXX", GlbStaff.CurrentUser.GS_Code);
	}

	DeclarationActivationHeader DeclarationActivationHeader => declarationActivationHeader ??= Factory.New<DeclarationActivationHeader>();
	DeclarationActivationHeader declarationActivationHeader;
}
