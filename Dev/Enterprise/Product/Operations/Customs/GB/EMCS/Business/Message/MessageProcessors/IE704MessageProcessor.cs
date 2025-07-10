using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE704MessageProcessor : EMCSMessageProcessor<IIE704>
	{
		public IE704MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("9E46B012-8853-4182-8332-2BCB956C4119", "EMCS IE704 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE704 provider)
		{
			if (message.EM_LinkedObject is EMCSJobDeclaration emcsDeclaration)
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Rejected;
				message.EM_Status = EDIMessage.Status.ProcessedOK;
				linkedEMCSDeclaration = emcsDeclaration;
				SendEmailNotification(message, Res.GetString("32EC10BE-2121-4D72-8E70-4A14247219F7", "EMCS Submission Rejected Response"), false, provider, null, GetEmailBody);
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE704)dataProvider;
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("6D3C432E-9914-44A7-A3E6-AC3809A316D3", "Your EMCS Declaration for Job {0} has been rejected. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("4DF0AF24-210F-4ED2-90C0-990C8BE71AB7", "ARC: {0}", provider.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("2793F4ED-38E6-4CC2-BFDB-6A732B04CA6A", "Error Details:"));

			var emailTable = new HtmlTableCreator(new string[]
			{
				Res.GetString("78E19961-46FC-48BB-A96F-4732FF55F1CF", "Error Location"),
				Res.GetString("F8E78F1E-5B56-425D-B1F0-8E774B83EA9E", "Error Type"),
				Res.GetString("7989719C-55A9-421E-A2F7-53D11CECCCB2", "Error Reason"),
				Res.GetString("4BD10867-9EA0-4F3A-BE15-2E380582F452", "Original Attribute Value"),
			});
			foreach (var line in provider.Errors)
			{
				emailTable.WriteRow(line.ErrorLocation,
					line.ErrorType + " - " + declaration.Factory.GetCachedValue<EMCSGBFunctionalErrorCodeList>().GetDescriptionFromCode(line.ErrorType),
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
