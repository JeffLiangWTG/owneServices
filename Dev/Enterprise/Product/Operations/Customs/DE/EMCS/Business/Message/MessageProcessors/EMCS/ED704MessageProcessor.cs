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
	public class ED704MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED704>, IED704>
	{
		public ED704MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("649326F7-5814-4E75-A1A2-18BA6C1189B7", "EMCS ED704 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED704> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.CorrelationIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED704> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Rejected;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			GenerateHtmlEmailAndSendToOriginalOrGroup(emcsDeclaration.Factory
				, emcsDeclaration
				, Res.GetString("75C43375-B3BC-49DA-8ACB-76223380DFCD", "EMCS Submission Rejected")
				, GetEmailBodyHeader(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, provider.CorrelationIdentifier);

			message.SetLogbookLocalReferenceNumber(provider.LocalReferenceNumber);
			var logbookRegistrationNumber = provider.AdministrativeReferenceCode.IsEmpty ? provider.CorrelationIdentifier : provider.AdministrativeReferenceCode;
			message.SetLogbookRegistrationNumber(logbookRegistrationNumber);
		}

		ZString GetEmailBodyHeader(EMCSJobDeclaration emcsDeclaration, IED704 dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("F0AE258C-909A-4C8F-A207-E114A579F420", "Your EMCS Declaration for Job {0} has been rejected. For details please follow the Link to the Job.", emcsDeclaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("775C98EC-E963-4541-B115-A62F32FA8FF2", "ARC: {0}", dataProvider.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("0F8D8F96-4288-4E39-BA28-47225DFEA43E", "Local Reference Number: {0}", dataProvider.LocalReferenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("A31ACDC0-6ECE-4110-996E-8B06379D66B4", "Error Details:"));

			var emailTable = new HtmlTableCreator(new string[]
			{
				Res.GetString("60AAF360-86AB-417F-9757-00B0F44B883E", "Error Number"),
				Res.GetString("180D136C-6F21-4F6B-A47F-F21675A592BF", "Line Number"),
				Res.GetString("0D626BEF-55E1-40E0-B598-490377BF4FEC", "Column Number"),
				Res.GetString("B907DC17-DF83-40BB-BDC5-D3D88E548929", "Error Type"),
				Res.GetString("37EAFB20-BD4A-4309-A761-05CE0FA81D7A", "Error Reason")
			});
			foreach (var line in dataProvider.Errors)
			{
				emailTable.WriteRow(line.ErrorNumber,
					line.LineNumber,
					line.ColumnNumber,
					line.ErrorType + " - " + emcsDeclaration.Factory.GetCachedValue<EmcsErrorTypeList>().GetDescriptionFromCode(line.ErrorType),
					line.ErrorReason);
			}

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(emailTable.ToHtml());

			return htmlBody.ToString();
		}
	}
}
