using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEDIMessageContentFilterManager
	{
		IEDIMessagePurpose EDIMessagePurpose { get; }
		IEDIMessageContentFilter EDIMessageContentFilter { get; }
	}
}
