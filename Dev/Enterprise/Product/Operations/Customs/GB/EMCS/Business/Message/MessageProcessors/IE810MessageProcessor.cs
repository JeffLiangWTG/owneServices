using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE810MessageProcessor : EMCSMessageProcessor<IIE810>
	{
		public IE810MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("C8F31F06-29ED-4497-B65F-2B34E86619E2", "EMCS IE810 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE810 provider)
		{
			if (message.EM_LinkedObject is EMCSJobDeclaration emcsDeclaration)
			{
				if (emcsDeclaration.IsConsignor)
				{
					emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
				}
				emcsDeclaration.JE_EntryStatus = EntryStatusList.Codes.CAN;
				linkedEMCSDeclaration = emcsDeclaration;
				SendEmailNotification(message, Res.GetString("615587EA-B65A-4F34-B979-3403DB1D6169", "EMCS Cancellation of e-AD"), false, provider, null, GetEmailBody);
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE810)dataProvider;
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("72A6F143-EDBB-4A49-BAD2-4ADCACFB35FD", "Your EMCS Declaration for Job {0} has been canceled. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("AD792954-B24A-4A57-B9C2-73E7684AF77F", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("3482639E-4529-41DD-8E59-210CCD70B75A", "Cancellation Reason: {0}", declaration.Factory.GetCachedValue<EMCSCancellationReasonList>().GetDescriptionFromCode(provider.CancellationReasonCode)));
			return emailBody.ToString();
		}
	}
}
