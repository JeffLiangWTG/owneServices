using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationExitOfficeProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationExitOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var declarationExitOffice = new DeclarationExitOfficeProvider(declaration);
			AssertEquals("Declaration Exit Office should be", ZString.Empty, declarationExitOffice.ID);
			declaration.BoardingOfficeCode = "1017500";
			AssertEquals("Declaration Exit Office should be", "1017500", declarationExitOffice.ID);
		}
	}
}
