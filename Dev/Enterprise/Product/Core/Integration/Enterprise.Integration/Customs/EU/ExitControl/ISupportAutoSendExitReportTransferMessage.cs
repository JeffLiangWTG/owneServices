using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ISupportAutoSendExitReportTransferMessage : Customs.IBaseAutoSendingMessageSupporter
			{
				IProcessor GetSendExitReportTransferMessageProcessor(BusinessObject parent);
			}
		}
	}
}
