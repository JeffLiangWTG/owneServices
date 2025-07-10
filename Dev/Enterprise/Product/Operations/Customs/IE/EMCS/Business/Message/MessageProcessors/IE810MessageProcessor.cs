using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE810MessageProcessor : EMCSMessageProcessor<IIE810>
	{
		public IE810MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5AEE7315-964C-483D-8D95-4795D39637C4", "EMCS IE810 Message Processor");

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
				SendEmailNotification(message, Res.GetString("482EF977-F55E-4FE6-B6FC-FCD9E34A37AE", "EMCS Cancellation of e-AD"), false, provider, null, GetEmailBody);
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE810)dataProvider;
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("05B365CE-3C5A-412F-9C84-E8E3A95388A3", "Your EMCS Declaration for Job {0} has been canceled. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("E07F1FD4-567E-4FB3-9EDF-78D5CFED2936", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("6F9DF1A3-5CC9-4CAB-B11C-52538487EA4C", "Cancellation Reason: {0}", declaration.Factory.GetCachedValue<EMCSCancellationReasonList>().GetDescriptionFromCode(provider.CancellationReasonCode)));
			return emailBody.ToString();
		}
	}
}
