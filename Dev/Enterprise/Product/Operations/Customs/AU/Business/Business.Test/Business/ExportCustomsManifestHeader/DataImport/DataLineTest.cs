using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DataLineTest : TestCaseWithFactory
	{
		public void TestSafeSubstring()
		{
			AssertEquals("012", dataLine.SafeSubstring("0123456789", 0, 3));
			AssertEquals("56789", dataLine.SafeSubstring("0123456789", 5, 100));
			AssertEquals("", dataLine.SafeSubstring("0123456789", 11, 3));
			AssertEquals("1", dataLine.SafeSubstring("1  2", 0, 3));
		}

		protected override void SetUp()
		{
			base.SetUp();
			dataLine = new FileHeaderLine(null, null, null);
		}

		DataLine dataLine;
	}
}
