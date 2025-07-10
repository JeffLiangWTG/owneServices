using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class VoyageDestinationAIRAARManager : CMRMessageManager
	{
		public VoyageDestinationAIRAARManager(CustomsVoyageDestinationWrapper destinationWrapper)
		{
			this.destinationWrapper = destinationWrapper;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { destinationWrapper.Calculator };

		internal override string GetStatus() => destinationWrapper.ActualArrivalStatus.Code;

		public override BusinessObject BusinessObject => destinationWrapper;

		public override string MessageFriendlyName => "Air Actual Arrival Report for " + destinationWrapper.PortOfArrival;

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new AIRAARMessageBuilder(bizo as CustomsVoyageDestinationWrapper) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CustomsVoyageDestinationWrapper).Messages;

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new VoyageDestinationAIRAARAmendmentGenerator(bizo as CustomsVoyageDestinationWrapper);

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			var newFactory = new BusinessObjectFactory();
			var destination = newFactory.Load<VoyageDestination>((businessObject as CustomsVoyageDestinationWrapper).Destination.PK);
			return destination != null ? new CustomsVoyageDestinationWrapper(new CustomsJobVoyageWrapper(destination.Voyage), destination) : null;
		}

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();
			if (destinationWrapper.ActualArrivalDateTimeUTC.IsEmpty || !destinationWrapper.ActualArrivalDateTimeUTC.IsValid)
			{
				result.AddError("You can't send this actual arrival report as the ATA has not been filled in.");
			}
			return result;
		}

		readonly CustomsVoyageDestinationWrapper destinationWrapper;
	}
}
