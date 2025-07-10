using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationOfficeProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var declarationOffice = new DeclarationOfficeProvider(declaration);
			AssertEquals("Declaration Office should be", ZString.Empty, declarationOffice.ID);
			declaration.JE_CustomsOffice = "0227700";
			AssertEquals("Declaration Office should be", "0227700", declarationOffice.ID);
		}
	}
}
