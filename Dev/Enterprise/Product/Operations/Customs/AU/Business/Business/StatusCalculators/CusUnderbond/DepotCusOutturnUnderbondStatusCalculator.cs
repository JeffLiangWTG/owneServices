
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusOutturnUnderbondStatusCalculator : CMRStatusCalculator<DepotCusOutturn>
	{
		public DepotCusOutturnUnderbondStatusCalculator(DepotCusOutturn outturn)
			: base(outturn)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.C5_MessageStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.UBMREQR };

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
	}
}
