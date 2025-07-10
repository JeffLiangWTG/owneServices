using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.MessagePurgeProcessor
{
	class InterchangeFilter
	{
		public const string MinMessageCreateTimeParamName = "minMessageCreateTime";
		public const string MaxMessageCreateTimeParamName = "maxMessageCreateTime";

		public InterchangeFilter(IEnumerable<ZString> applicationCodes, ZDateTime minMessageCreateTimeUtc, ZDateTime maxMessageCreateTimeUtc)
		{
			MinMessageCreateTimeUtc = minMessageCreateTimeUtc;
			MaxMessageCreateTimeUtc = maxMessageCreateTimeUtc;
			ApplicationCodes = applicationCodes;
			Condition = BuildCondition(ApplicationCodes);
		}

		public IEnumerable<ZString> ApplicationCodes { get; private set; }

		public ZDateTime MinMessageCreateTimeUtc { get; set; }

		public ZDateTime MaxMessageCreateTimeUtc { get; private set; }

		public string Condition { get; private set; }

		static string BuildCondition(IEnumerable<ZString> applicationCodes)
		{
			var filter = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, applicationCodes);
			filter.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, FakeMaxCreateTime);
			filter.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, FakeMinCreateTime);

			return ReplaceFakeValuesByParameters(filter.LiteralTextSqlFormatted);
		}

		static string ReplaceFakeValuesByParameters(string value)
		{
			const string format = "'{0:yyyy-MM-dd HH:mm:ss.fff}'";
			return value
				.Replace(string.Format(CultureInfo.InvariantCulture, format, FakeMinCreateTime), "@" + MinMessageCreateTimeParamName)
				.Replace(string.Format(CultureInfo.InvariantCulture, format, FakeMaxCreateTime), "@" + MaxMessageCreateTimeParamName);
		}

		static readonly DateTime FakeMinCreateTime = new DateTime(1984, 1, 14, 12, 34, 56, 789);
		static readonly DateTime FakeMaxCreateTime = new DateTime(1985, 10, 8, 12, 34, 56, 789);
	}
}