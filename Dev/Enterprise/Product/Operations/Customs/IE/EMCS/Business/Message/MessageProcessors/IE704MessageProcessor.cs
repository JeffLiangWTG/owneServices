using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE704MessageProcessor : EMCSMessageProcessor<IIE704>
	{
		public IE704MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("f0c2c04c-9920-46e8-b159-9ea3a21c4e5e", "EMCS IE704 Message Processor");

		protected override Type MessageInterpreterType => typeof(IE704MessageInterpreter);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE704 provider)
		{
			if (message.EM_LinkedObject is EMCSJobDeclaration emcsDeclaration)
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Rejected;
				message.EM_Status = EDIMessage.Status.ProcessedOK;

				linkedEMCSDeclaration = emcsDeclaration;
				SendEmailNotification(message, Res.GetString("71272a1a-30ff-4a4b-b253-c5a6684162ad", "EMCS Submission Rejected Response"), false, provider, null, GetEmailBody);
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE704)dataProvider;
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("aaa45f14-1bdc-4e0a-8d58-fc0ab3cb5ba3", "Your EMCS Declaration for Job {0} has been rejected. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("c8a0fbff-552f-44b3-84a4-de617da46d31", "ARC: {0}", provider.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("6dbbf1ea-ef7a-4f1d-b822-68c095e8a297", "Error Details:"));

			var emailTable = new HtmlTableCreator(new string[]
			{
				Res.GetString("867e4063-9e9f-4e46-bfe7-42ed8c5ec5c9", "Error Location"),
				Res.GetString("c77bd3e0-b5ae-4689-91c9-0cbc2bd482fb", "Error Type"),
				Res.GetString("a33c803c-915b-476a-8f3e-73b13b34bdf2", "Error Reason"),
				Res.GetString("457f7d7e-21a2-4a6a-a3c5-c53d64b00aaa", "Original Attribute Value"),
			});
			foreach (var line in provider.Errors)
			{
				emailTable.WriteRow(line.ErrorLocation,
					line.ErrorType + " - " + declaration.Factory.GetCachedValue<EMCSFunctionalErrorCodeList>().GetDescriptionFromCode(line.ErrorType),
					line.ErrorReason,
					line.OriginalAttributeValue);
			}

			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(emailTable.ToHtml());
			return emailBody.ToString();
		}
	}
}
