using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE803MessageProcessor : EMCSMessageProcessor<IIE803>
	{
		public IE803MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("44CD43CB-4808-4254-9065-6B02071C0DF7", "EMCS IE803 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE803 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.DIV;

			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("5B238A0A-980C-4D83-B110-D8BB67B015AD", "EMCS Notification of diverted e-AD"), false, provider, null, GetEmailBody);
		}

		protected override ZString ReplaceExtraXMLElementIfNeeded(ZString text)
		{
			return text.Replace(":NotificationOfDivertedEAD>", ":NotificationOfDivertedEADESAD>");
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE803)dataProvider;
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("71357438-7610-4257-8FB3-F84E9CA9A532", @"Your EMCS Declaration for Job {0} received a notification of diverted e-AD. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("BFE7A676-7216-45B9-AECB-93E474411119", @"Notification Date/Time: {0}", provider.NotificationDateTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("1376D279-D878-40AF-8141-2F18D84E017A", @"Notification Type: {0} - {1}", provider.NotificationType, declaration.Factory.GetCachedValue<EMCSNotificationType>().GetDescriptionFromCode(provider.NotificationType)));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("5E48CB83-5616-449F-ADA6-6C3DE60727E3", @"ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("636597AA-F5A9-48A0-ACDE-993F48530BAD", @"Sequence Number: {0}", provider.ExciseMovementEad.SequenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			if (provider.DownstreamARCs.Any())
			{
				var htmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("7459B0D3-FFDC-41DA-AC10-D4B0BEFB351F", "Downstream ARC") });
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
