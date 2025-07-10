using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE819MessageProcessor : EMCSMessageProcessor<IIE819>
	{
		public IE819MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("BD505C12-EE67-4EA5-80E1-792D9ABA505F", "EMCS IE819 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE819 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.ALT;
			if (emcsDeclaration.IsConsignee)
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			}

			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("3482205C-1FEC-4A49-A216-1895AA1BD8CF", "EMCS Notification of Alert or Rejection"), false, provider, null, GetEmailBody);
		}

		protected override ZString ReplaceExtraXMLElementIfNeeded(ZString text)
		{
			return text.Replace(":ExciseMovementEad>", ":ExciseMovement>").Replace(":AlertOrRejectionOfEadReason>", ":AlertOrRejectionOfEadEsadReason>")
				.Replace(":EadRejectedFlag>", ":EadEsadRejectedFlag>").Replace(":AlertOrRejectionOfEadReasonCode>", ":AlertOrRejectionOfMovementReasonCode>");
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent exciseMovementEad)
		{
			var provider = (IIE819)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("22F97779-083F-48C2-B04E-5EF6001CC7DE", "Your EMCS Declaration for Job {0} received an Alert or Rejection. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("351EB6CC-F7CF-4A4A-A62C-9AC6CBBDA096", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));

			if (provider.AlertOrRejectionReasons.Any())
			{
				var emailTable = new HtmlTableCreator(new[]
				{
					Res.GetString("79551221-9EC0-4C76-951B-C8AF9E67CCC2", "Reason"),
					Res.GetString("E5AC1149-D725-4B10-86C4-EECF4B2ADDD5", "Additional Information")
				});
				foreach (var alertOrRejectionReason in provider.AlertOrRejectionReasons)
				{
					emailTable.WriteRow(alertOrRejectionReason.ReasonCode + " - " + declaration.Factory.GetCachedValue<EU.EMCS.Business.EMCSAlertRejectionCodeList>().GetDescriptionFromCode(alertOrRejectionReason.ReasonCode)
						, alertOrRejectionReason.ComplementaryInformation);
				}
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(emailTable.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}
