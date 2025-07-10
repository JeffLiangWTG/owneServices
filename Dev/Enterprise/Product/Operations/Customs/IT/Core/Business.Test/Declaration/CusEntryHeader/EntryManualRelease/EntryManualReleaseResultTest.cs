using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

internal sealed class EntryManualReleaseResultTest : TestCase
{
	public void TestProperties()
	{
		var result = new EntryManualReleaseResult(canDoManualRelease: true, errorMessage: "Sic transit");

		CombineAssertions("Both fields populated", () =>
		{
			AssertEquals("same value as in constructor expected for CanDoManualRelease", true, result.CanDoManualRelease);
			AssertEquals("same value as in constructor expected for ErrorMessage", "Sic transit", result.ErrorMessage);
		});

		result = new EntryManualReleaseResult(canDoManualRelease: false, errorMessage: ZString.Empty);
		AssertEquals("when errorMessage is empty, empty string is expected", ZString.Empty, result.ErrorMessage);

		result = new EntryManualReleaseResult(canDoManualRelease: false, errorMessage: null);
		AssertEquals("when errorMessage is null, empty string is expected", ZString.Empty, result.ErrorMessage);
	}
}
