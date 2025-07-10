using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class OriginalB3TransactionNumberCodeFindBoxTest : TestCaseWithFactory
	{
		public void TestIFindBoxMembers()
		{
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.JE_DeclarationReference = "B12345678";

			Factory.Save();

			var b3FindBox = new OriginalB3TransactionNumberCodeFindBox();
			((IFindBox)b3FindBox).Code = "B12345678";
			AssertEquals(declaration.TransactionNumber.ToString(), ((IFindBox)b3FindBox).Code);
			b3FindBox.Dispose();
		}
	}
}
