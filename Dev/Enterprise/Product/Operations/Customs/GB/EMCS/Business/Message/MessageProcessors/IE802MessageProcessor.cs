using System;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE802MessageProcessor : EMCSMessageProcessor<IIE802>
	{
		public IE802MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("4C0E65CC-A547-40A3-BC11-8A405E6AA4F3", "EMCS IE802 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE802 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.REM;
			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("FD5E98FA-684F-4C5C-AB89-B9EB5D4E3B2F", "EMCS Reminder Message"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE802)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("96875FEE-4F5B-49CF-B29E-D25C51040BC3", "Your EMCS Declaration for Job {0} received a reminder message. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("D430B4B9-C585-4ED4-95B1-51FE3C2AEB8B", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("80FC4EE3-8232-470F-BCAD-1E3F2FF9E882", "Line No.: {0}", provider.ExciseMovementEad.SequenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("6F7D91B0-14BD-4236-953B-6CAD2E2C9947", "Limit Date Time: {0}", provider.LimitDateTime.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture)));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			if (!provider.ReminderInformation.IsEmpty)
			{
				htmlBody.Append(Res.GetString("AA66B742-35A3-4FE9-8840-6F64035ADCAC", "Reminder Information: {0}", provider.ReminderInformation));
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
			}
			htmlBody.Append(Res.GetString("C594DD79-3D98-4847-A61E-51A0DB88E40D", "Reminder Message Type: {0} - {1}", provider.ReminderMessageType, declaration.Factory.GetCachedValue<EMCSGBReminderMessageType>().GetDescriptionFromCode(provider.ReminderMessageType)));

			return htmlBody.ToString();
		}
	}
}
