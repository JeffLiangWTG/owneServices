using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class EnquireyResultCountMessageTest : TestCase
	{
		public void TestGetTooManyResultsErrorString()
		{
			EnquireyResultCountMessageTesting resultCount = new EnquireyResultCountMessageTesting(null, 500, 200);
			AssertEquals("Error message should be as follows: ", "Too many records (1000). Please contact admin to increase the max. number of records displayed.",
				resultCount.GetTooManyResultsErrorMessageExposed(1000));
		}
	}
}
