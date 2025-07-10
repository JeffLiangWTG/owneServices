
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondStatusCalculator : CMRStatusCalculator<CusUnderbond>
	{
		public CusUnderbondStatusCalculator(CusUnderbond underbond) : base(underbond)
		{
		}

		protected internal override ZString GetStatusFromInboundMessage(EDIMessage incomingMessage)
		{
			if (incomingMessage is CMRUBMREQRMessage)
			{
				return (incomingMessage as CMRUBMREQRMessage).GetStatusCode();
			}
			else
			{
				return base.GetStatusFromInboundMessage(incomingMessage);
			}
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.UnderbondStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes
			=> new ZString[] { CMRMessage.CMRMessageTypes.UBMREQ, CMRMessage.CMRMessageTypes.UBMREQE, CMRMessage.CMRMessageTypes.UBMREQR };

		protected override void DeriveStatusCore()
		{
			base.DeriveStatusCore();
			if (!Parent.IsDeleted && Parent.UnderbondStatus.Code != CMRBaseStatuses.Codes.OriginalRejected && Parent.UnderbondStatus.Code != CMRBaseStatuses.Codes.AwaitingResponseToOriginal && Parent.C4_Status == CMRUnderbondStatuses.Codes.UnderbondSendingDelayed)
			{
				Parent.C4_Status = ZString.Empty;
			}
		}
	}
}
