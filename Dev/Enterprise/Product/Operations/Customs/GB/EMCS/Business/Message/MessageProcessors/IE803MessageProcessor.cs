using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE803MessageProcessor : EMCSMessageProcessor<IIE803>
	{
		public IE803MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("D7EA7236-6F62-4A4D-A255-509C9563568D", "EMCS IE803 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE803 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.DIV;
			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("A8D690ED-D16F-40D9-AAC6-6F708D31BF64", "EMCS Notification of diverted e-AD"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE803)dataProvider;
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("C570D193-1E6B-49EC-8DF4-622E3E7B2DF1", @"Your EMCS Declaration for Job {0} received a notification of diverted e-AD. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("740F72E1-05EC-4B69-A64D-063C4F8B9F76", @"Notification Date/Time: {0}", provider.NotificationDateTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("7F7681E6-23D5-4F81-9B1F-835C325752F0", @"Notification Type: {0} - {1}", provider.NotificationType, declaration.Factory.GetCachedValue<EMCSGBNotificationType>().GetDescriptionFromCode(provider.NotificationType)));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("94DCDD6B-084F-4C46-91C4-B080050AA013", @"ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("20C51982-471E-4102-8224-ACBBE8443392", @"Sequence Number: {0}", provider.ExciseMovementEad.SequenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			if (provider.DownstreamARCs.Any())
			{
				var htmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("09E079D6-BB25-44CC-9BE1-73676B6C1F9C", "Downstream ARC") });
				foreach (var downstreamArc in provider.DownstreamARCs)
				{
					htmlTableCreator.WriteRow(downstreamArc);
				}
				htmlBody.Append(htmlTableCreator.ToHtml());
			}
			return htmlBody.ToString();
		}
	}
}
