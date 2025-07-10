namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXMLDataProvider
	{
		long Timestamp { get; }
		bool IgnoreTimestamp { get; }

#if DEBUG
		void SetUseDefaultValueForTest(bool useDefault);
		void SetIgnoreTimestampForTest(bool ignoreTimestamp);
#endif
	}
}
