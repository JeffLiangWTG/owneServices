using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class FileNameValidatorTest : TestCase
	{
		public void TestIsFileNameValid()
		{
			AssertEquals(false, FileNameValidator.IsFileNameValid(null));
			AssertEquals(false, FileNameValidator.IsFileNameValid(string.Empty));
			AssertEquals(false, FileNameValidator.IsFileNameValid("*"));
			AssertEquals(false, FileNameValidator.IsFileNameValid("hello\\"));
			AssertEquals(true, FileNameValidator.IsFileNameValid("hello"));
		}
	}
}