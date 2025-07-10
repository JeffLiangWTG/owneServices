using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// AU Customs has a bug for SAC with lines and with a flag, 'Line Liability breakdown' on, it does not report Line Level Charges at all
	/// When the issue is addressed, this needs to be inherited from CMRHeaderAndLineChargesResponseProcessor
	/// </summary>
	public class SACRMessageProcessor : CMRHeaderChargesResponseProcessor
	{
		public SACRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.SAC, "Self Assessed Clearance Declaration Response (SACR)")
		{
		}
	}
}
