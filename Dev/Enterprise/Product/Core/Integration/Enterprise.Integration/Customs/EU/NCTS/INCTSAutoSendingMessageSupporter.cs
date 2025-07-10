using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public static partial class NCTS
			{
				public interface INCTSAutoSendingMessageSupporter : IBaseAutoSendingMessageSupporter
				{
					ZString GetReasonForNotSupportNCTSMessage { get; }
					IProcessor CreateNCTSMessageProcessor();
					IProcessor CreateNCTSArrivalNotificationMessageProcessor();
				}

				public interface INctsHeaderWithAdditionalMessagingValidation
				{
					string[] GetAdditionalValidationErrorMessages();
				}
			}
		}
	}
}
