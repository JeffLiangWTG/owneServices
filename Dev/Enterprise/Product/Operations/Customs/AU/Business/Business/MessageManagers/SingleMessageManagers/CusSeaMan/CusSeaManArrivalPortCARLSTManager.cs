using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortCARLSTManager : CMRMessageManager
	{
		public CusSeaManArrivalPortCARLSTManager(CusSeaManArrivalPort arrival)
		{
			this.arrival = arrival;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { arrival.CargoListStatusCalculator };

		internal override string GetStatus() => arrival.CargoListStatus.Code;

		public override BusinessObject BusinessObject => arrival;

		public override string MessageFriendlyName => "Cargo List Report for Port - " + arrival.BA_RL_NKArrivalPort;

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new CARLSTMessageBuilder(bizo as CusSeaManArrivalPort) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusSeaManArrivalPort).Messages;

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusSeaManArrivalPortCARLSTAmendmentGenerator(bizo as CusSeaManArrivalPort);

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			Customs.Business.MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();
			if (arrival.CargoLines.Count < 1)
			{
				result.AddError("At least one cargo list line is required.");
			}

			return result;
		}

		readonly CusSeaManArrivalPort arrival;
	}
}
