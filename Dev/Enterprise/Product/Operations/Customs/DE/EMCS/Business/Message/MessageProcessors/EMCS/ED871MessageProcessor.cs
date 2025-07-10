using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED871MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED871>, IED871>
	{
		public ED871MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("D2DE182A-E132-41A4-8BBD-D069EBB3F3CA", "EMCS ED871 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED871> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var exciseMovement = provider.ExciseMovement;
				result = GetDeclarationFromEADNumber(message, exciseMovement.AdministrativeReferenceCode, provider.MessageGroup, exciseMovement.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED871> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.SHR;
			emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			foreach (var line in provider.Lines)
			{
				var invoiceLine = emcsDeclaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().SingleOrDefault(x => x.JI_LineNo == ZShort.ParseSafe(line.LineNumber, ZShort.Zero));
				if (invoiceLine != null)
				{
					invoiceLine.ZG_ExciseProductCode = line.ExciseProductCode;
					invoiceLine.Outturn.C5_OutturnResultReason = line.Explanation;
				}
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("602C6AB6-B80E-403B-82D2-319F134B63DA", "EMCS Shortage or Excess Explanation Received")
				, GetEmailBodyHeader(emcsDeclaration.JE_DeclarationReference
				, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ExciseMovement.AdministrativeReferenceCode);
		}

		ZString GetEmailBodyHeader(ZString declarationReference, IED871 dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("B5F7C0FF-A492-4BB2-BE23-0E8771A5797B", "Your EMCS Declaration for Job {0} received a Shortage or Excess Explanation. For details please follow the Link to the Job.", declarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("E2BE9979-9F1D-4028-85FB-219A908B7A83", "ARC: {0}", dataProvider.ExciseMovement.AdministrativeReferenceCode));

			if (!dataProvider.GlobalExplanation.IsEmpty)
			{
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("107DF894-D8F3-49BA-BB71-1914F334F313", "Global Explanation: {0}", dataProvider.GlobalExplanation));
			}
			if (dataProvider.Lines.Any())
			{
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("A0475EBF-7B78-4A03-BE61-FF7F894C6A1D", "Analysis Detail:"));

				var emailTable = new HtmlTableCreator(new string[]
				{
				Res.GetString("F3C6BB66-2440-44AD-9EB8-5734C44FF1F3", "Position"),
				Res.GetString("15FC7F5E-FA72-481B-8211-22610B8A3537", "Excise Product Code"),
				Res.GetString("2958AAA5-B509-43DB-8FFE-8046EDAFF71E", "Actual Quantity"),
				Res.GetString("97F8B5A5-3B55-44CD-99F9-1B47474DB670", "Explanation"),
				});
				foreach (var line in dataProvider.Lines)
				{
					emailTable.WriteRow(line.LineNumber,
						line.ExciseProductCode,
						line.ActualQuantity.ToString(),
						line.Explanation);
				}

				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(emailTable.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}
