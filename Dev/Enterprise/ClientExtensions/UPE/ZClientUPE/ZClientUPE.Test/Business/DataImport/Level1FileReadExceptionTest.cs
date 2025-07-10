using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1FileReadExceptionTest : TestCase
	{
		public void TestLevel1FileReadException()
		{
			Level1FileReadException level1FileReadException = new Level1FileReadException();

			level1FileReadException = new Level1FileReadException("TEST");
			AssertEquals("TEST", level1FileReadException.Message);

			level1FileReadException = new Level1FileReadException("TEST2", level1FileReadException);
			AssertEquals("TEST2", level1FileReadException.Message);
			AssertEquals("TEST", level1FileReadException.InnerException.Message);
		}
	}
}
