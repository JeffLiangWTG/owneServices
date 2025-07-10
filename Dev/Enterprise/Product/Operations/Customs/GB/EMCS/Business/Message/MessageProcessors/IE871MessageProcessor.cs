using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE871MessageProcessor : EMCSMessageProcessor<IIE871>
	{
		public IE871MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("CFA26872-A92D-4DCD-AF7D-88223E6C933F", "EMCS IE871 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE871 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.SHR;
			emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			linkedEMCSDeclaration = emcsDeclaration;
			foreach (var line in provider.Lines)
			{
				var invoiceLine = emcsDeclaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().SingleOrDefault(x => x.JI_LineNo == ZShort.ParseSafe(line.LineNumber, ZShort.Zero));
				if (invoiceLine != null)
				{
					invoiceLine.ZG_ExciseProductCode = line.ExciseProductCode;
					invoiceLine.Outturn.C5_OutturnResultReason = line.Explanation;
				}
			}

			SendEmailNotification(message, Res.GetString("1CB50E69-754E-42C4-A646-C03CA3974A20", "EMCS Shortage or Excess Explanation Received"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE871)dataProvider;
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("09A24AB9-CD2D-450F-A2E7-3B22B9BC2866", "Your EMCS Declaration for Job {0} received a Shortage or Excess Explanation. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("CE1E4310-B15A-4085-BD34-8A0EBAE4495E", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));

			if (!provider.GlobalExplanation.IsEmpty)
			{
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("1F11E22A-349D-4E46-8CFE-DEBDA82B635B", "Global Explanation: {0}", provider.GlobalExplanation));
			}
			if (provider.Lines.Any())
			{
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("5676C26B-AA3D-491F-8E30-490EC2849021", "Analysis Detail:"));

				var emailTable = new HtmlTableCreator(new string[]
				{
				Res.GetString("15EABC78-4F11-4EF9-982D-1F58F066FC21", "Position"),
				Res.GetString("5CF39556-45D2-4E19-B7C5-CEA66260B8B0", "Excise Product Code"),
				Res.GetString("5EE040CB-FE11-480C-890E-A797A498E4DF", "Actual Quantity"),
				Res.GetString("D692752B-AA04-4B7F-894A-7BA5A15C4FF2", "Explanation"),
				});
				foreach (var line in provider.Lines)
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
