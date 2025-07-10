using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgCusCode))]
	sealed class EDIOrgCusCodeTest : OrgCusCodeTest
	{
		public void TestUPEOrgCusCode_Code()
		{
			var expectedError = "Enter a valid Type.";

			var ediOrgCusCode = Factory.New<EDIOrgCusCode>();
			ediOrgCusCode.OK_CodeType = "ABM";
			ediOrgCusCode.Validation.ValidateOK_CodeType();
			AssertNoError(ediOrgCusCode.OK_CodeTypeInfo, expectedError);

			ediOrgCusCode.OK_CodeType = "AAA";
			ediOrgCusCode.Validation.ValidateOK_CodeType();
			AssertHasError(ediOrgCusCode.OK_CodeTypeInfo, expectedError);
		}
	}
}
