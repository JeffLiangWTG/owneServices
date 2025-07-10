using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	public class RecordHeaderLineTest : TestCase
	{
		[TestDate(2001, 03, 03, 12, 34, 10)]
		public void TestLineAsString()
		{
			AssertEquals("2001-03-0312:34:1000025500012000056" + new string(' ', 256), Line.LineAsString);
		}

		RecordHeaderLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new RecordHeaderLine(255, 12, 56);
				}

				return fLine;
			}
		}

		RecordHeaderLine fLine;
	}
}
