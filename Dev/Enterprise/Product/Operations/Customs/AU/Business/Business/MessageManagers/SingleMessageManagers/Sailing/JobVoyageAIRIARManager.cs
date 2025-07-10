using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobVoyageAIRIARManager : CMRMessageManager
	{
		public JobVoyageAIRIARManager(CustomsJobVoyageWrapper voyageWrapper)
		{
			this.voyageWrapper = voyageWrapper;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { voyageWrapper.Calculator };

		internal override string GetStatus() => voyageWrapper.ImpendingArrivalStatus.Code;

		public override BusinessObject BusinessObject => voyageWrapper;

		public override string MessageFriendlyName => "Air Impending Arrival Report";

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new AIRIARMessageBuilder(bizo as CustomsJobVoyageWrapper) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CustomsJobVoyageWrapper).Messages;

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();
			if (voyageWrapper.ResponsiblePartyID.IsEmpty)
			{
				result.AddError("You have not entered a Responsible Party ID. Please ensure you have filled out the 'Business Reg No' on your company 'Org. Proxy'.");
			}

			return result;
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new JobVoyageAIRIARAmendmentGenerator(bizo as CustomsJobVoyageWrapper);

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject) => CustomsJobVoyageWrapper.Load(new BusinessObjectFactory(), (businessObject as CustomsJobVoyageWrapper).Voyage.PK);

		readonly CustomsJobVoyageWrapper voyageWrapper;
	}
}
