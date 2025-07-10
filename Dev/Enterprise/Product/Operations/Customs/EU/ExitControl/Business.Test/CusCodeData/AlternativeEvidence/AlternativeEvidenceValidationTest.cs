using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	public class AlternativeEvidenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var alternativeEvidence = Factory.New<CusExitReport>().AlternativeEvidences.AddNew();
			alternativeEvidence.CY_Code = ZString.Empty;
			AssertHasMessageErrorContaining(alternativeEvidence.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(alternativeEvidence.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			alternativeEvidence.CY_Code = "!!";
			AssertNoMessageErrorContaining(alternativeEvidence.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(alternativeEvidence.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
