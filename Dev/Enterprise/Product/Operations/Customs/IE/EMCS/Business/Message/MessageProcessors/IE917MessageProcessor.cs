using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE917MessageProcessor : EMCSMessageProcessor<IIE917>
	{
		public IE917MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("38E4F10A-0980-4F6F-A900-D0BD06224B9F", "EMCS IE917 Message Processor");

		protected override Type MessageInterpreterType => typeof(IE917MessageInterpreter);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE917 provider)
		{
			if (message.EM_LinkedObject is EMCSJobDeclaration emcsDeclaration)
			{
				emcsDeclaration.JE_MessageStatus = LogicalStatusList.Codes.Error;

				linkedEMCSDeclaration = emcsDeclaration;
				SendEmailNotification(message, Res.GetString("7DC9A79F-FCE2-4B67-B06A-8E29F279E05C", "EMCS negative acknowledgement of XML message"), false, provider, null, GetEmailBody);
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent exciseMovementEad)
		{
			var provider = (IIE917)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("5AE6E83A-D429-46D6-BC91-E75620919D1F", "Your EMCS Declaration for Job {0} received a negative acknowledgement of XML message. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("1310D0BC-89F8-4D6B-A936-4A67512710A4", "ARC: {0}", provider.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("2945AB00-BAE3-4421-8E7F-A11460683DBA", "Error Details:"));

			var emailTable = new HtmlTableCreator(new string[]
			{
				Res.GetString("0782C81D-3A38-4C72-84E2-7255731F6F70", "Error Line Number"),
				Res.GetString("74A58A32-E32B-4A2B-988C-D7D7CD771A60", "Error Column Number"),
				Res.GetString("218CF70D-6AEA-46E8-A670-A3BD3978260C", "Error Reason"),
				Res.GetString("7C07B4BA-0D57-4691-A0AF-6290A0837367", "Error Location"),
				Res.GetString("8270F746-DD2A-4A57-AAA2-649C66744A71", "Original Attribute Value")
			});
			foreach (var line in provider.Errors)
			{
				emailTable.WriteRow(line.ErrorLineNumber,
					line.ErrorColumnNumber,
					line.ErrorReason,
					line.ErrorLocation,
					line.OriginalAttributeValue);
			}

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(emailTable.ToHtml());

			return htmlBody.ToString();
		}
	}
}
