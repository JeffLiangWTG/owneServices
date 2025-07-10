using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.DE.Business
{
	public class DEOutboundInterchangeProvider : InterchangeProviderBase
	{
		public DEOutboundInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		protected override string GetCollationKey(EDIMessage message) => InterchangeProviderBase.DoNotCollateType;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = messages.Cast<EDIMessage>().Single();
			CustomsGenericMessageHelper.PopulateGMDInterchange(interchange, DECustomsDataRegistry.Instance.ZNetRecipientID.Value, message.EM_ApplicationCode, message.EM_MessageText);
			interchange.EI_InterchangeNum = message.EM_MessageNum;
			interchange.ContainedMessages.Add(message);
			message.EM_Status = EDIMessage.Status.Sent;
		}

		//The SenderID is the License Key, this exists for all systems.
		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		//We have no footer text everything is in the boody
		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;
	}
}
