using System;
using System.Threading;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public class XMLDataProvider : IXMLDataProvider
	{
#if DEBUG
		static AsyncLocal<bool> UseDefaultValueForTest { get; } = new AsyncLocal<bool>();

		static AsyncLocal<bool> IgnoreTimestampForTest { get; } = new AsyncLocal<bool>();

		public void SetUseDefaultValueForTest(bool useDefault)
		{
			UseDefaultValueForTest.Value = useDefault;
		}

		public void SetIgnoreTimestampForTest(bool ignoreTimestamp)
		{
			IgnoreTimestampForTest.Value = ignoreTimestamp;
		}
#endif
		public long Timestamp
		{
			get
			{
#if DEBUG
				if (UseDefaultValueForTest.Value)
				{
					return XMLDataProviderConstant.DefaultTimestamp;
				}
#endif
				return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			}
		}

		public bool IgnoreTimestamp
		{
			get
			{
#if DEBUG
				if (IgnoreTimestampForTest.Value)
				{
					return true;
				}
#endif
				return false;
			}
		}
	}
}
