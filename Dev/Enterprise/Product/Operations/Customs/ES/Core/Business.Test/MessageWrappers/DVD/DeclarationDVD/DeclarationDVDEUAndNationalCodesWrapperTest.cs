using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDEUAndNationalCodesWrapperTest : WrapperHelperTest<DeclarationDVDEUAndNationalCodesWrapper>
	{
		public void TestEUCode()
		{
			AssertEquals("Expected filled EUCode", "EUCode", wrapper.EUCode);
		}
		public void TestEUCodeSecondConstructor()
		{
			wrapper = new DeclarationDVDEUAndNationalCodesWrapper("A123");
			AssertEquals("Expected filled EUCode when the code starts with a letter", "A123", wrapper.EUCode);
		}

		public void TestNationalCode()
		{
			AssertEquals("Expected filled NationalCode", "NationalCode", wrapper.NationalCode);
		}

		public void TestNationalCodeSecondConstructor()
		{
			wrapper = new DeclarationDVDEUAndNationalCodesWrapper("1ABC");
			AssertEquals("Expected filled NationalCode when the code starts with a number", "1ABC", wrapper.NationalCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new DeclarationDVDEUAndNationalCodesWrapper("EUCode", "NationalCode");
		}

		DeclarationDVDEUAndNationalCodesWrapper wrapper;

		protected override DeclarationDVDEUAndNationalCodesWrapper GetProvider() => wrapper;
	}
}
