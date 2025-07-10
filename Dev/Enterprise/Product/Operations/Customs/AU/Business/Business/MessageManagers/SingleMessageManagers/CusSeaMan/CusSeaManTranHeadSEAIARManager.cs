using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadSEAIARManager : CMRMessageManager
	{
		public CusSeaManTranHeadSEAIARManager(CusSeaManTranHead transportHeader)
		{
			this.transportHeader = transportHeader;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { transportHeader.Calculator };

		internal override string GetStatus() => transportHeader.ImpendingArrivalResponseStatus.Code;

		public override BusinessObject BusinessObject => transportHeader;

		public override string MessageFriendlyName => "Impending Arrival Report";

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new SEAIARMessageBuilder(bizo as CusSeaManTranHead) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusSeaManTranHead).Messages;

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusSeaManTranHeadSEAIARAmendmentGenerator(bizo as CusSeaManTranHead);

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();
			if (transportHeader.Arrivals.Count < 1)
			{
				result.AddError("At least one port of arrival is required.");
			}
			return result;
		}

		readonly CusSeaManTranHead transportHeader;
	}
}
