using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCInterchangeProvider : OneInterchangeToOneMessageInterchangeProvider
	{
		public NEXDOCInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID => string.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = messages[0];
			var recipientId = message.EM_IsTestMessage ? Constants.DataProvider.NEXDOCSTest : Constants.DataProvider.NEXDOCS;

			SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, recipientId, GlbCompany.CurrentCompany.LicenceKeyIdentifier);

			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.NEXDOCS;
			interchange.EI_GB = message.EM_GB;
		}

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange)
		{
			return EDIInterchangeStatusList.Codes.eHubQueued;
		}

		protected override StringBuilder MessageBody(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			return new StringBuilder(GetInterchangeBodyText(messages, interchange));
		}

		string GetInterchangeBodyText(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var messageText = base.MessageBody(messages, interchange).ToString();
			var message = messages[0];
			var interchangeData = new NEXDOCAcknowledgeInterchange(interchange.EI_From, interchange.EI_To, message.EM_SystemCreateUser, MessageType, message.EM_ApplicationReference, ZString.Empty, messageText);

			return interchangeData.Serialize();
		}

		const string MessageType = "RexAcknowledgeOwnership";

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}
	}
}
