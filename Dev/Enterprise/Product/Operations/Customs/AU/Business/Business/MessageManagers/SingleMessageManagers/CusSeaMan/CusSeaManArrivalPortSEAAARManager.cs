using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortSEAAARManager : CMRMessageManager
	{
		public CusSeaManArrivalPortSEAAARManager(CusSeaManArrivalPort arrival)
		{
			this.arrival = arrival;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { arrival.ActualArrivalStatusCalculator };

		internal override string GetStatus() => arrival.ActualArrivalResponseStatus.Code;

		public override BusinessObject BusinessObject => arrival;

		public override string MessageFriendlyName => "Actual Arrival Report (" + arrival.BA_RL_NKArrivalPort + ")";

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new SEAAARMessageBuilder(bizo as CusSeaManArrivalPort) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusSeaManArrivalPort).Messages;

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusSeaManArrivalPortSEAAARAmendmentGenerator(bizo as CusSeaManArrivalPort);

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();

			if (arrival.BA_ArrivalPortATA.IsEmpty || !arrival.BA_ArrivalPortATA.IsValid)
			{
				var error = new Customs.Business.MessageSendingError("You cannot send unless ATA Port Information has been set.");
				result.Add(error);
			}

			return result;
		}

		readonly CusSeaManArrivalPort arrival;
	}
}
