using System.Collections.Concurrent;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[ThreadSafe]
	class RefDbByTypeAndCountryDictionary
	{
		public RefDbByTypeAndCountryDictionary()
		{
		}

		public string GetReferenceDatabaseName(RefDbTypeEnum refDbType, string refCountryCode)
		{
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));

			string result = null;
			var dbTypeAndCountryKey = GetRefDbKey(refDbType, refCountryCode);
			internalDictionary.TryGetValue(dbTypeAndCountryKey, out result);
			return result;
		}

		public void SetReferenceDatabaseName(RefDbTypeEnum refDbType, string refCountryCode, string refDbName)
		{
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));

			string dbTypeAndCountryKey = GetRefDbKey(refDbType, refCountryCode);
			internalDictionary[dbTypeAndCountryKey] = refDbName;
		}

		string GetRefDbKey(RefDbTypeEnum refDbType, string refCountryCode)
		{
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));

			return string.Format("{0}.{1}", refDbType, refCountryCode);
		}

		readonly ConcurrentDictionary<string, string> internalDictionary = new ConcurrentDictionary<string, string>();
	}
}
