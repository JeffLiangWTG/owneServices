using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.MessagePurgeProcessor
{
	class ApplicationCodeFilterCreator : BaseFilterCreator
	{
		public ApplicationCodeFilterCreator(List<ZString> applicationCodes, ZDateTime minMessageCreateTimeUtc, ZDateTime maxMessageCreateTimeUtc)
			: base(minMessageCreateTimeUtc, maxMessageCreateTimeUtc)
		{
			ApplicationCodes = applicationCodes;
			Condition = BuildCondition();
		}

		public List<ZString> ApplicationCodes { get; private set; }

		protected override string BuildCondition()
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes);
			filter.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, FakeMaxCreateTime);
			filter.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, FakeMinCreateTime);
			return ReplaceFakeValuesByParameters(filter.LiteralTextSqlFormatted);
		}
	}
}
