using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class TRLRLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			AssertEquals("TRLR3100", Line.LineAsString);
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 0;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.TRLR;
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new TRLRLine();
		}
	}
}
