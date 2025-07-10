using System.Collections.Generic;
using System.Linq;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IKeysResult
	{
		bool IsMatch { get; }

		IEnumerable<(string KeyValue, string KeySource)> KeysInfo { get; }
		IEnumerable<string> Keys { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Key results are not comparable.")]
	public struct KeysResult : IKeysResult
	{
		KeysResult(bool isMatch, IEnumerable<(string KeyValue, string KeySource)> keysInfo)
		{
			IsMatch = isMatch;
			KeysInfo = keysInfo;
		}

		public bool IsMatch { get; }
		public IEnumerable<(string KeyValue, string KeySource)> KeysInfo { get; }

		public IEnumerable<string> Keys => KeysInfo.Select(k => k.KeyValue);

		public static KeysResult ForceSequentialOrdering((string KeyValue, string KeySource) keysInfo) => new KeysResult(isMatch: true, keysInfo: new[] { keysInfo });
		public static KeysResult NoMatch() => new KeysResult(isMatch: false, keysInfo: Enumerable.Empty<(string KeyValue, string KeySource)>());
		public static KeysResult Match(IEnumerable<(string KeyValue, string KeySource)> keysInfo) => new KeysResult(isMatch: true, keysInfo: keysInfo);
	}
}
