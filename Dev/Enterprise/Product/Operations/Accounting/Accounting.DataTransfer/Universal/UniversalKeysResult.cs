using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer
{
	internal class UniversalKeysResult : IKeysResult
	{
		public UniversalKeysResult(IEnumerable<(string KeyValue, string KeySource)> keysInfo)
		{
			KeysInfo = keysInfo;
			IsMatch = true;
		}

		public UniversalKeysResult()
		{
		}

		public IEnumerable<(string KeyValue, string KeySource)> KeysInfo { get; set; }
		public bool IsMatch { get; set; }
		public IEnumerable<string> Keys => KeysInfo.Select(k => k.KeyValue);
	}
}
