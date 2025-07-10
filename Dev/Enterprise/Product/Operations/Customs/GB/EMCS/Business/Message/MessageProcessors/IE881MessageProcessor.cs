using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE881MessageProcessor : EMCSMessageProcessor<IIE881>
	{
		public IE881MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("0A26639F-71ED-4870-A16E-25A63087969A", "EMCS IE881 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE881 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			if (provider.ManualClosureResponse.ManualClosureRequestAccepted)
			{
				emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.MAN;
			}
			linkedEMCSDeclaration = emcsDeclaration;

			SendEmailNotification(message, Res.GetString("733C0670-1B94-4153-929D-1E93E2FE22B0", "EMCS Manual Closure {0}", provider.ManualClosureResponse.ManualClosureRequestAccepted ? "Accepted" : "Rejected"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE881)dataProvider;
			var requestAccepted = provider.ManualClosureResponse.ManualClosureRequestAccepted;
			var declarationReference = declaration.JE_DeclarationReference;
			var htmlBody = new StringBuilder();
			htmlBody.Append(requestAccepted
				? Res.GetString("64B56207-7330-40AC-95BE-4DCCA1D8310A", "Your EMCS Declaration for Job {0} has been closed manually. For details please follow the link to the Job.", declarationReference)
				: Res.GetString("3C89CE61-07FC-41BB-915F-1545EAEBD60B", "Your request for a Manual Closure of Job {0} has been rejected. For details please follow the link to the Job.", declarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("F77154CF-6190-41DA-A120-9274419FF525", "Sending Customs Office: {0}", provider.MessageSender));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("F7144FAF-809D-4682-88DB-58D0BFBF2027", "MRN: {0}", provider.MrnNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("9C7F5203-FED1-48A0-8E9D-8279214D2646", "Sequence Number: {0}", provider.MrnNumberSequenceNumber));

			if (!requestAccepted)
			{
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				var rejectionReasonList = new EMCSGBManualClosureRejectionReasonList();
				var reason = provider.ManualClosureResponse.ManualClosureRejectionReasonCode;
				htmlBody.Append(Res.GetString("CC379A17-6E9D-4700-99D3-5E2EDA3FAAFF", "Manual Closure Rejection Reason: {0} - {1}", reason, rejectionReasonList.GetDescriptionFromCode(reason)));
				if (reason == EMCSGBManualClosureRejectionReasonList.Codes.Reason0)
				{
					htmlBody.Append("<br />");
					htmlBody.Append("<br />");
					htmlBody.Append(Res.GetString("668B6238-11C7-484C-B145-CEFA67BE3080", "Rejection Complement: {0}", provider.ManualClosureResponse.ManualClosureRejectionComplement.Text));
				}
			}

			return htmlBody.ToString();
		}
	}
}
