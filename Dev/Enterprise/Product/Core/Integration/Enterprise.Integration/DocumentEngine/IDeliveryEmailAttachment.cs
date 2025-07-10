namespace Enterprise.Integration.DocumentEngine
{
	public interface IDeliveryEmailAttachment
	{
		string FileName { get; }
		long FileSizeInBytes { get; }
		bool ShouldBeAttached { get; }
	}
}
