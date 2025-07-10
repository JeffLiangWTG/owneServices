using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts
{
	public class CTCGB5InterchangeProvider : CTCInterchangeProvider
	{
		public CTCGB5InterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages) : base(logger, messages)
		{
		}

		protected override ZString GetHeaderText(EDIMessage message)
		{
			var request = GovernmentGatewayExtensions.NewGBCustomsRequest(message);
			if (request != null && IsLargeFile(message))
			{
				request.LargeFile = "true";
			}
			return request?.Serialize() ?? ZString.Empty;
		}

		bool IsLargeFile(EDIMessage message)
		{
			var configLargeFileSize = new RefSysConfig.Loader(message.Factory).GetDecimalValue("GBCTC5LFS", 5000000);
			var messageSize = Encoding.UTF8.GetBytes(message.Interchange.EI_BodyText).LongLength;
			return messageSize >= configLargeFileSize;
		}

		protected override ZString GetInterchangeType(EDIMessage message) => message.EM_MessageType;
	}
}
