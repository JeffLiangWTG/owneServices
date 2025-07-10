using System;
using CargoWise.Types;

namespace Enterprise.ServiceManager.Tasks.MessagePurgeProcessor
{
	abstract class BaseFilterCreator
	{
		public const string MinMessageCreateTimeParamName = "minMessageCreateTime";
		public const string MaxMessageCreateTimeParamName = "maxMessageCreateTime";

		public BaseFilterCreator(ZDateTime minMessageCreateTimeUtc, ZDateTime maxMessageCreateTimeUtc)
		{
			MinMessageCreateTimeUtc = minMessageCreateTimeUtc;
			MaxMessageCreateTimeUtc = maxMessageCreateTimeUtc;
		}

		public ZDateTime MinMessageCreateTimeUtc { get; set; }

		public ZDateTime MaxMessageCreateTimeUtc { get; private set; }

		public string Condition { get; protected set; }

		protected abstract string BuildCondition();

		protected string ReplaceFakeValuesByParameters(string value)
		{
			const string format = "'{0:yyyy-MM-dd HH:mm:ss.fff}'";
			return value
				.Replace(string.Format(format, FakeMinCreateTime), "@" + MinMessageCreateTimeParamName)
				.Replace(string.Format(format, FakeMaxCreateTime), "@" + MaxMessageCreateTimeParamName);
		}

		protected readonly DateTime FakeMinCreateTime = new DateTime(1984, 1, 14, 12, 34, 56, 789);
		protected readonly DateTime FakeMaxCreateTime = new DateTime(1985, 10, 8, 12, 34, 56, 789);
	}
}
