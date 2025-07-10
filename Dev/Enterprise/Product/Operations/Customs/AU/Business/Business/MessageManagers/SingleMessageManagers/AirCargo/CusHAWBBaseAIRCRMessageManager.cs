using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusHAWBBaseAIRCRManager : CMRMessageManager
	{
		public CusHAWBBaseAIRCRManager(CusHAWBBase hawb)
		{
			this.hawb = hawb;
		}

		protected override bool RequiresAmendmentCore() => hawb.IsMasterBillChangedFromEmpty || base.RequiresAmendmentCore();

		protected override bool ShouldWaitUntilResponded => base.ShouldWaitUntilResponded && !hawb.IsMasterBillChangedFromEmpty;

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { hawb.Calculator, hawb.MessageStatusCalculator };

		internal override string GetStatus() => hawb.CMRMessageStatus.Code;

		protected override bool ShouldValidateCurrentCompanyLocalBusNumCharacters => true;

		protected override void ResetToOriginalCore()
		{
			base.ResetToOriginalCore();
			hawb.CS_IsResponsePending = false;
			if (hawb.MAWB != null)
			{
				hawb.MAWB.CM_HouseMessageIsSent = false;
			}
		}

		public override BusinessObject BusinessObject => hawb;

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusHAWBBase).Messages;

		readonly CusHAWBBase hawb;
	}
}
