using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE819MessageProcessor : EMCSMessageProcessor<IIE819>
	{
		public IE819MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("D8EDC709-BD8E-47C6-A79A-8C1CE9BC788F", "EMCS IE819 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE819 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.ALT;
			if (emcsDeclaration.IsConsignee)
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			}

			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("FE551BCF-3F25-4CC0-98A4-BBB52DC70EA9", "EMCS Notification of Alert or Rejection"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent exciseMovementEad)
		{
			var provider = (IIE819)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("35729A2C-79F4-4EA5-9D50-23D5D7FFB6F7", "Your EMCS Declaration for Job {0} received an Alert or Rejection. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("9B39E461-C9B5-42ED-AADE-35473740BF06", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));

			if (provider.AlertOrRejectionReasons.Any())
			{
				var emailTable = new HtmlTableCreator(new[]
				{
					Res.GetString("C49E0905-60D4-4680-84D1-E43BB12E9C01", "Reason"),
					Res.GetString("8188F597-42BA-4C83-94F6-E00FE051E298", "Additional Information")
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
