using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class InstallationResultTest : TestCase
	{
		InstallationResult Result;

		public void TestOK()
		{
			Result = InstallationResult.OK();
			AssertEquals(InstallationResultStatus.OK, Result.Status);
			AssertEquals(string.Empty, Result.Message);
			AssertEquals(false, Result.AffectCargoWiseFunctions);
		}

		public void TestOKWithMessage()
		{
			Result = InstallationResult.OK("message");
			AssertEquals(InstallationResultStatus.OK, Result.Status);
			AssertEquals("message", Result.Message);
			AssertEquals(false, Result.AffectCargoWiseFunctions);
		}

		public void TestIsOK()
		{
			Result = InstallationResult.OK();
			Assert(Result.IsOK);
			Assert(!Result.IsWarning);
			Assert(!Result.IsError);
		}

		public void TestIsOKWithMessage()
		{
			Result = InstallationResult.OK("message");
			Assert(Result.IsOK);
			Assert(!Result.IsWarning);
			Assert(!Result.IsError);
		}

		public void TestWarning()
		{
			Result = InstallationResult.Warning("some warning");
			AssertEquals(InstallationResultStatus.Warning, Result.Status);
			AssertEquals("some warning", Result.Message);
			AssertEquals(true, Result.AffectCargoWiseFunctions);
		}

		public void TestNotAffectCargoWiseFunctionsWarning()
		{
			Result = InstallationResult.NotAffectCargoWiseFunctionsWarning("some warning");
			AssertEquals(InstallationResultStatus.Warning, Result.Status);
			AssertEquals("some warning", Result.Message);
			AssertEquals(false, Result.AffectCargoWiseFunctions);
			Assert(!Result.IsOK);
			Assert(Result.IsWarning);
			Assert(!Result.IsError);
		}

		public void TestIsWarning()
		{
			Result = InstallationResult.Warning("warning");
			Assert(!Result.IsOK);
			Assert(Result.IsWarning);
			Assert(!Result.IsError);
		}

		public void TestError()
		{
			Result = InstallationResult.Error("some error");
			AssertEquals(InstallationResultStatus.Error, Result.Status);
			AssertEquals("some error", Result.Message);
			AssertEquals(true, Result.AffectCargoWiseFunctions);
		}

		public void TestIsError()
		{
			Result = InstallationResult.Error("error");
			Assert(!Result.IsOK);
			Assert(!Result.IsWarning);
			Assert(Result.IsError);
		}
	}
}
