using System;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE802MessageProcessor : EMCSMessageProcessor<IIE802>
	{
		public IE802MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("D5687FF2-0027-43C3-98ED-B300A8160F8B", "EMCS IE802 Message Processor");

		protected override Type MessageInterpreterType => typeof(IE802MessageInterpreter);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE802 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.REM;

			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("CA07CED9-86C9-4975-866C-5613280DE6A6", "EMCS Reminder Message"), false, provider, null, GetEmailBody);
		}

		protected override ZString ReplaceExtraXMLElementIfNeeded(ZString text)
		{
			return text.Replace(":ExciseMovementEad>", ":ExciseMovement>");
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE802)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("8626B8B4-B8D3-49A4-9CBD-0D809D1E679F", "Your EMCS Declaration for Job {0} received a reminder message. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("3E34D953-583E-4910-A660-50981CC8E8FE", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("90C9D560-8B13-4CD6-81A3-1A3F75639EE0", "Line No.: {0}", provider.ExciseMovementEad.SequenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("AA4C3B3F-74F3-47B9-A0C2-D8EA18E627E8", "Limit Date Time: {0}", provider.LimitDateTime.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture)));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			if (!provider.ReminderInformation.IsEmpty)
			{
				htmlBody.Append(Res.GetString("6A917CD4-BE7C-426C-9CC1-783720FD3BF0", "Reminder Information: {0}", provider.ReminderInformation));
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
			}
			htmlBody.Append(Res.GetString("FC7781E4-6902-47CF-95C7-243737AB7EAA", "Reminder Message Type: {0} - {1}", provider.ReminderMessageType, declaration.Factory.GetCachedValue<EMCSReminderMessageType>().GetDescriptionFromCode(provider.ReminderMessageType)));

			return htmlBody.ToString();
		}
	}
}
