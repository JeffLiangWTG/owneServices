using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class EXTREJMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXTREJ>, IEXTREJ>
	{
		public EXTREJMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E686B327-8CAC-43B1-926E-EDC55EA23FBC", "Export EXTREJ Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXTREJ> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXTREJ> message)
		{
			var cusExitReport = (CusExitReport)message.EM_LinkedObject;
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			var exitHeader = cusExitReport.Header;
			var dataProvider = message.DataProvider;
			var status = dataProvider.ConsignmentStatus;
			cusExitReport.CER_Status = status;

			var consignment = cusExitReport.Consignment;
			consignment.CXC_Status = status;
			consignment.Logs.AddNew(AutoEvents.CustomsEntryStatus, status);

			cusExitReport.Logs.AddNew(Events.CustomsEntryStatus, cusExitReport.CER_Status, ZDateTime.Now.ToOffset());

			var body = CreateEmailBody(dataProvider, cusExitReport);
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, exitHeader, Res.GetString("B74986A7-080E-464D-8BA7-57490F3AE90C", "AES EXT Rejection Message"), body, false, message.Branch, exitHeader, dataProvider.ReferencedMessageIdentifier);

			message.SetLogbookRegistrationNumber(message.DataProvider.MovementReferenceNumber);
		}

		string CreateEmailBody(IEXTREJ provider, CusExitReport report)
		{
			var emailBody = new StringBuilder();

			emailBody.Append(Res.GetString("463FC974-DF9D-4B2A-9AFF-291EF9675A4F",
				"Your Exit Control Message for {0} has a Rejection Message. For details, please follow the Link to the Job.",
				report.Header.CXH_JobReference));

			emailBody.Append((NoResString)"<br/><br/>");

			var emailTable = new HtmlTableCreator();
			emailTable.WriteRow(Res.GetString("51D77261-5C03-4483-A98D-1F3EEF88AB2C", "MRN"), provider.MovementReferenceNumber);
			emailTable.WriteRow(Res.GetString("34E636B2-F806-4254-9F2D-C6C826D250BF", "LRN"), provider.LocalReferenceNumber);
			emailTable.WriteRow(Res.GetString("3D354AB2-0FED-448D-B676-361F0CC00CD4", "Status"), report.CER_Status);
			emailTable.WriteRow(Res.GetString("33455DF0-672B-43BF-872A-92E1A5BA2456", "Status Text"), report.StatusDescription);
			emailBody.Append(emailTable.ToHtml());

			return emailBody.ToString();
		}
	}
}
