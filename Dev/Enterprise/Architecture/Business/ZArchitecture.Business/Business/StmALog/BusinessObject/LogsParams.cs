using System.Collections.Generic;
using System.Dynamic;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class LogsParams : DynamicObject, ILogsParams
	{
		public LogsParams(string reference)
		{
			#if DEBUG
			if (EventReferenceProcessor.IsValidReferenceForTest(reference))
			#endif
			{
				logsParams = StmALog.GetParametersFromReference(reference);
			}
			REF = StmALog.GetFreeTextFromReference(reference);
		}

		public string REF { get; }

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			string value;

			if (!logsParams.TryGetValue(binder.Name, out value))
			{
				value = string.Empty;
			}

			result = value;
			return true;
		}

		readonly IDictionary<string, string> logsParams;
	}
}
