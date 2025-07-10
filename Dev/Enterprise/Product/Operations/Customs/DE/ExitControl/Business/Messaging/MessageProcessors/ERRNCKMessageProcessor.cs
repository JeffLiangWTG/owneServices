using System.Text;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ERRNCKMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IERRNCK>, IERRNCK>
	{
		public ERRNCKMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4DC6BAD6-B90D-4CF1-BB3A-AEA6CFB989D9", "Export ERRNCK Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IERRNCK> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IERRNCK> message)
		{
			var provider = message.DataProvider;
			var report = message.EM_LinkedObject as CusExitReport;

			var header = report.Header;

			report.CER_MessageStatus = LogicalStatusList.Codes.Error;

			SendEmail();

			var logbookRegistrationNumber = !provider.ReferenceNumber.IsNullOrEmpty() ? provider.ReferenceNumber : provider.ReferencedMessageIdentifier;
			message.SetLogbookRegistrationNumber(logbookRegistrationNumber);
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			report.Logs.AddNew(Events.CustomsEntryStatus, UniversalReferenceConstants.EntryStatus.ERR, ZDateTime.Now.ToOffset());

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, header
						, Res.GetString("A688B096-7F41-4156-A291-F96516FB34BB", $"Exit Declaration Message Status")
						, GetEmailBody()
						, false
						, message.Branch
						, header
						, provider.ReferencedMessageIdentifier);

				string GetEmailBody()
				{
					var htmlBody = new StringBuilder();
					htmlBody.Append(Res.GetString("31EC0709-95D5-4E05-A352-4CABB23F1D10", "Your Exit Declaration Message for Job {0} has been rejected. For details please follow the Link to the Job.", header.CXH_JobReference));
					htmlBody.Append("<br /><br />");
					htmlBody.Append(GetErrorsEmailTable(provider.Errors));

					return htmlBody.ToString();
				}
			}
		}
	}
}
