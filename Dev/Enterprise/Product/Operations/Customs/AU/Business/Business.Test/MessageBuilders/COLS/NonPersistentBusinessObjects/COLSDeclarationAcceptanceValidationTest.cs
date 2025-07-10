using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSDeclarationAcceptanceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDeclarationAcceptance()
		{
			var messageError = "You have not ticked the check box.";
			var acceptanceBO = new COLSDeclarationAcceptance();
			acceptanceBO.DeclarationAcceptance = false;
			acceptanceBO.Validation.ValidateDeclarationAcceptance();
			AssertHasMessageError(acceptanceBO.DeclarationAcceptanceInfo, messageError);

			acceptanceBO.DeclarationAcceptance = true;
			acceptanceBO.Validation.ValidateDeclarationAcceptance();
			AssertNoMessageError(acceptanceBO.DeclarationAcceptanceInfo, messageError);
		}
	}
}
