using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Integration.Testing
{
	public class CommonUtilsTest : TestCaseWithFactory
	{
		public void TestGetLetterRepresentationRange()
		{
			for (var num = 1; num < 703; num++)
			{
				AssertNoExceptionThrown(num.ToString(), () => CommonUtils.GetLetterRepresentation(num));
			}
		}

		public void TestGetLetterRepresentationRange_Under()
		{
			AssertEquals(string.Empty, CommonUtils.GetLetterRepresentation(0));
			AssertEquals(string.Empty, CommonUtils.GetLetterRepresentation(-1));
		}

		public void TestGetLetterRepresentationRange_Over()
		{
			AssertExceptionThrown("703", typeof(ArgumentOutOfRangeException), () => CommonUtils.GetLetterRepresentation(703));
		}

		public void TestUniqueID()
		{
			AssertEquals(1, CommonUtils.GetNumberRepresentation("A"));
			AssertEquals(3, CommonUtils.GetNumberRepresentation("C"));
			AssertEquals(29, CommonUtils.GetNumberRepresentation("AC"));
			AssertEquals(0, CommonUtils.GetNumberRepresentation(""));
			AssertEquals(1, CommonUtils.GetNumberRepresentation("/A"));
			AssertEquals(26, CommonUtils.GetNumberRepresentation("/Z"));
			AssertEquals(27, CommonUtils.GetNumberRepresentation("/AA"));
			AssertEquals(53, CommonUtils.GetNumberRepresentation("/BA"));
			AssertEquals(78, CommonUtils.GetNumberRepresentation("/BZ"));
			AssertExceptionThrown(typeof(ArgumentNullException), () => CommonUtils.GetNumberRepresentation(null));

			AssertEquals("DZ", CommonUtils.GetLetterRepresentation(130));
			AssertEquals("", CommonUtils.GetLetterRepresentation(0));
		}

		public void TestValidaeSuffixForGetNumberRepresentation()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => CommonUtils.ValidaeSuffixForGetNumberRepresentation(null));

			AssertEquals("", CommonUtils.ValidaeSuffixForGetNumberRepresentation(""));
			AssertEquals("", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/A"));
			AssertEquals("", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/Q"));
			AssertEquals("", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/AA"));
			AssertEquals("", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/ZX"));
			AssertEquals("", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/ZZ"));

			AssertEquals("Invalid suffix.", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/"));

			string expectedErrorMessage = "Suffix must start with: '/'.";
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("A/Z=Xh"));
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("A"));
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("AA"));
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("ZZ"));
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("ZZZ"));
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("AAA"));

			expectedErrorMessage = "Suffix conatins invalid characters: ";
			AssertEquals(expectedErrorMessage + "'1'.", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/1"));
			AssertEquals(expectedErrorMessage + "'123'.", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/C123"));
			AssertEquals(expectedErrorMessage + "'/=h'.", CommonUtils.ValidaeSuffixForGetNumberRepresentation("/A/Z=Xh"));

			expectedErrorMessage = "Maximum length of suffixes is exceeded.";
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("/ZAX"));
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("/ZZZ"));
			AssertEquals(expectedErrorMessage, CommonUtils.ValidaeSuffixForGetNumberRepresentation("/AAA"));
		}
	}
}
