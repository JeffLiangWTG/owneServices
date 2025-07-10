using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing
{
	public class ParserTest : TestCaseWithFactory
	{
		#region Setup

		class FakeLogger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => Enumerable.Empty<ISimpleLog>();

			public void Log(LogType type, string message)
			{
			}
		}

		#endregion

		public void TestValidDateRanges()
		{
			AssertNull(new ZDateTimeParser().TryParseAndValidate("0021-09-01 10:00", typeof(ZDateTime), "Christmas", new FakeLogger()));
			AssertNull(new ZDateTimeOffsetParser().TryParseAndValidate("0021-09-01 10:00", typeof(ZDateTimeOffset), "Christmas", new FakeLogger()));

			AssertEquals(new ZDateTime(2021, 9, 1, 10, 0, 0), new ZDateTimeParser().TryParseAndValidate("2021-09-01 10:00", typeof(ZDateTime), "Christmas", new FakeLogger()));
			AssertEquals(new ZDateTimeOffset(2021, 9, 1, 10, 0, 0), new ZDateTimeOffsetParser().TryParseAndValidate("2021-09-01 10:00", typeof(ZDateTimeOffset), "Christmas", new FakeLogger()));
		}
	}
}
