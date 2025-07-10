using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IJobDeclarationAutoSendingMessageSupporter : IBaseAutoSendingMessageSupporter
		{
			ZBool SupportEntryDeclarationMessage { get; }
			ZBool SupportReleaseMessage { get; }

			ZString GetReasonForNotSupportEntryDeclarationMessage { get; }
			ZString GetReasonForNotSupportReleaseMessage { get; }

			IProcessor CreateEntryDeclarationMessageProcessor();
			IProcessor CreateReleaseMessageProcessor(string eventCode = "");
		}
	}
}
