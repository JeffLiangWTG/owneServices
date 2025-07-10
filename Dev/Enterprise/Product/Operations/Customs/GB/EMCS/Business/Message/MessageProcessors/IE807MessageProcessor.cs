using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE807MessageProcessor : EMCSMessageProcessor<IIE807>
	{
		public IE807MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("78E197AE-6BFC-4EFD-A5C2-C834F4F2FE5A", "EMCS IE807 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE807 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.INT;
			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("14B0E284-1D20-467F-A693-76F387623C97", "EMCS Interruption of Movement"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE807)dataProvider;
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("C97F1C5C-CF3E-43C0-ACA0-FF8B61FF3254", "Your EMCS Declaration for Job {0} received a notification that the movement has been interrupted. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var headerTableCreator = new HtmlTableCreator();
			var exciseMovementEad = provider.ExciseMovementEad;
			headerTableCreator.WriteRow(Res.GetString("B442185C-25C4-4EF6-B27D-C77D67A929F5", "ARC"), exciseMovementEad.AdministrativeReferenceCode);
			headerTableCreator.WriteRow(Res.GetString("1637094B-7B0A-4676-A940-D8D5F5A0C8CE", "Reason"), exciseMovementEad.Reason + " - " + new EMCSGBInterruptionReasonList().GetDescriptionFromCode(exciseMovementEad.Reason));
			if (!exciseMovementEad.ComplementaryInformation.IsEmpty)
			{
				headerTableCreator.WriteRow(Res.GetString("B1D1B0B7-3543-4302-B92C-E94D56F66969", "Complementary Information"), exciseMovementEad.ComplementaryInformation);
			}
			htmlBody.Append(headerTableCreator.ToHtml());

			if (provider.ControlReportNumbers.Any())
			{
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("FCCDF8A7-45D6-4151-B7C8-C19CA366DD33", "Control Report"));
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				var controlReportTableCreator = new HtmlTableCreator(new[] { Res.GetString("919E121C-4582-408C-8A2C-717006332287", "Control Report Number") });
				foreach (var controlReportNumber in provider.ControlReportNumbers)
				{
					controlReportTableCreator.WriteRow(controlReportNumber);
				}
				htmlBody.Append(controlReportTableCreator.ToHtml());
			}
			if (provider.EventReportNumbers.Any())
			{
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("9AFE90D8-0AA1-434E-BEFA-0B3E34D4FD2D", "Event Report"));
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				var eventReportTableCreator = new HtmlTableCreator(new[] { Res.GetString("D8DFB3D3-4B64-42A9-8647-3206827164B5", "Event Report Number") });
				foreach (var eventReportNumber in provider.EventReportNumbers)
				{
					eventReportTableCreator.WriteRow(eventReportNumber);
				}
				htmlBody.Append(eventReportTableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}

