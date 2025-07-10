using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class TCPStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public TCPStatusCalculator() { }

		public override ZString MessageTypeDescription => MessageTypeList.Descriptions.TradeChainPartner;

		public static string CalculateTCPStatus(string tcpStatus, ProcessingIndicatorDescriptionCodeList emailStatus)
		{
			switch (tcpStatus)
			{
				case CSAStatusList.Codes.AwaitingAdd:
					{
						if (emailStatus == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)
						{
							return CSAStatusList.Codes.Added;
						}
						else if (emailStatus == ProcessingIndicatorDescriptionCodeList.ErrorMessage)
						{
							return CSAStatusList.Codes.ErrorAdded;
						}
						else
						{
							return tcpStatus;
						}
					}
				case CSAStatusList.Codes.AwaitingDelete:
					{
						if (emailStatus == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)
						{
							return CSAStatusList.Codes.Deleted;
						}
						else if (emailStatus == ProcessingIndicatorDescriptionCodeList.ErrorMessage)
						{
							return CSAStatusList.Codes.ErrorDeleted;
						}
						else
						{
							return tcpStatus;
						}
					}
				default:
					return tcpStatus;
			}
		}

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			return ZString.Empty;
		}
	}
}
