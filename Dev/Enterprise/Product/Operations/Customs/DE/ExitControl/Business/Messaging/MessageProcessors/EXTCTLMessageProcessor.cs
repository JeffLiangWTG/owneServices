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
	public class EXTCTLMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXTCTL>, IEXTCTL>
	{
		public EXTCTLMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E3DBFDB0-36FF-4DD2-AAFE-98277E71BD26", "Export EXTCTL Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXTCTL> message) =>
			GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXTCTL> message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			if (message.EM_LinkedObject is CusExitReport cusExitReport)
			{
				cusExitReport.CER_Status = A0116ATLASStatusCodeList.Codes._351;
				cusExitReport.Logs.AddNew(Events.CustomsEntryStatus, cusExitReport.CER_Status, ZDateTime.Now.ToOffset());

				var consignment = cusExitReport.Consignment;
				if (consignment != null)
				{
					consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._351;
					CreateEvent(consignment);
				}

				var body = CreateEmailBody(message.DataProvider, cusExitReport);
				var subject = Res.GetString("A6499E43-E1FE-4D99-84EF-14EF27B39AB7", "AES EXT Control Measure Message");

				GenerateHtmlEmailAndSendToOriginalOrGroup(factory, cusExitReport.Header, subject, body, false, message.Branch, cusExitReport.Header, message.DataProvider.ReferencedMessageIdentifier);
			}
			else
			{
				status = EDIMessageStatusList.Codes.Error;
			}

			message.SetLogbookRegistrationNumber(message.DataProvider.MovementReferenceNumber);
			message.EM_Status = status;
		}

		void CreateEvent(EU.ExitControl.Business.CusExitConsignment consignment)
		{
			consignment.GetLogs().AddNew(AutoEvents.CustomsEntryStatus, A0116ATLASStatusCodeList.Codes._351);
		}

		string CreateEmailBody(IEXTCTL provider, CusExitReport report)
		{
			var emailBody = new StringBuilder();

			emailBody.Append(Res.GetString("45A07B75-0A70-4672-B5AC-A1315E2CA655",
				"Your Exit Control Message for {0} has a control measure request. For details, please follow the Link to the Job.",
				report.Header.CXH_JobReference));

			emailBody.Append((NoResString)@"<br/><br/>");
			emailBody.Append(Res.GetString("E72E3F9B-FB86-4411-A996-613EB48073BD", "MRN: {0}", provider.MovementReferenceNumber));
			emailBody.Append((NoResString)@"<br/><br/>");

			var emailTable = new HtmlTableCreator(new[]
			{
				Res.GetString("0615499E-65A5-4AF5-B4AC-81E66FE3C4B1", "Control Type"),
				Res.GetString("BDB88FAA-4CBC-4E03-A781-80EC0E2F4A97", "Annotation"),
			});

			foreach (var typeofControls in provider.TypeOfControls)
			{
				var controlType = typeofControls.Type;
				var description = c0716TypeOfControlsCodeList.GetDescriptionFromCode(controlType);
				emailTable.WriteRow($"{controlType} - {description}", typeofControls.Text);
			}

			emailBody.Append(emailTable.ToHtml());

			return emailBody.ToString();
		}

		readonly ReadOnlyCodeDescriptionPairList c0716TypeOfControlsCodeList = new C0716TypeOfControlsCodeList();
	}
}
