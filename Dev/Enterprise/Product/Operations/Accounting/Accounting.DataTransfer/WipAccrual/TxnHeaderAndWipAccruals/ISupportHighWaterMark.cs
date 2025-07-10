using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer
{
	public interface ISupportHighWaterMark
	{
		bool IsHighWaterMarkEnabled { get; }
		DateTimeRegistryItem HighWaterMarkRegistry { get; }
	}
}
