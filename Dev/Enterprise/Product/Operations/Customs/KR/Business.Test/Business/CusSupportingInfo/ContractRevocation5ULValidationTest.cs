using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ContractRevocation5ULValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidation()
		{
			var supportingInfo = Factory.New<ContractRevocation5UL>();
			AssertEquals(typeof(ContractRevocation5ULValidation), supportingInfo.Validation.GetType());
		}

		public void TestCSI_Code()
		{
			var contractRevocation5UL = Factory.New<ContractRevocation5UL>();
			ValidationTestHelper.AssertInvalidCodeMessageError(contractRevocation5UL.CSI_CodeInfo, new ZString[] { "E" }, new ZString[] { "A", "B", "C", "D" });
		}
	}
}
