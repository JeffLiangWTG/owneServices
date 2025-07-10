using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class RSFStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public RSFStatusCalculator() { }

		public override ZString MessageTypeDescription => MessageTypeList.Descriptions.CSARevenueSummaryForm;

		public static string CalculateRSFStatus(string rsfStatus, ProcessingIndicatorDescriptionCodeList emailStatus)
		{
			var result = ZString.Empty;
			switch (rsfStatus)
			{
				case MessageStatusList.Codes.AwaitingOriginal:
					if (emailStatus == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)
					{
						result = MessageStatusList.Codes.AcknowledgedOriginal;
					}
					else if (emailStatus == ProcessingIndicatorDescriptionCodeList.ErrorMessage)
					{
						result = MessageStatusList.Codes.ErrorOriginal;
					}
					else
					{
						result = rsfStatus;
					}
					break;
				case MessageStatusList.Codes.AwaitingChange:
					if (emailStatus == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)
					{
						result = MessageStatusList.Codes.AcknowledgedChange;
					}
					else if (emailStatus == ProcessingIndicatorDescriptionCodeList.ErrorMessage)
					{
						result = MessageStatusList.Codes.ErrorChange;
					}
					else
					{
						result = rsfStatus;
					}
					break;
				default:
					result = rsfStatus;
					break;
			}
			return result;
		}

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			return ZString.Empty;
		}
	}
}
