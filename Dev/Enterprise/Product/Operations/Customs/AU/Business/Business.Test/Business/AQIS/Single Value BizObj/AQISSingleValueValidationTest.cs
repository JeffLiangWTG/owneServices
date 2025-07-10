using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AQISSingleValueValidationTest : TestCaseWithFactory
	{
		public void TestValidateIncorrectCode()
		{
			foreach (char charNotAllowed in BizObjToTest.Validation.CharsNotAllowed)
			{
				BizObjToTest.Code = charNotAllowed.ToString();
				AssertHasErrors("You cannot use the characters '" + charNotAllowed + "'", BizObjToTest.CodeInfo);
			}

			BizObjToTest.Code = "A";
			AssertNoErrors("You cannot use the characters 'A'", BizObjToTest.CodeInfo);
		}

		public abstract void TestCodeAgainstLookupList();

		public abstract void TestNumberOfCodesEntered();

		public abstract AQISSingleValueBusinessObject BizObjToTest { get; }
	}
}
