using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class VoyageDestinationAIRAARManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestBusinessObject()
		{
			AssertEquals(DestinationWrapper, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Air Actual Arrival Report for ", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(DestinationWrapper);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRAARMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(DestinationWrapper);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRAARMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(DestinationWrapper);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRAARMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((VoyageDestinationAIRAARManager)Manager).GetStatus());
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, ((VoyageDestinationAIRAARManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(VoyageDestinationActualArrivalStatusCalculator), ((VoyageDestinationAIRAARManager)Manager).StatusCalculators[0].GetType());
		}

		public void TestGetBusinessObjectInNewFactory()
		{
			DestinationWrapper.Factory.Save();
			var newDestinationWrapper = (CustomsVoyageDestinationWrapper)(((VoyageDestinationAIRAARManagerForTest)GetManager()).GetBusinessObjectInNewFactory(DestinationWrapper));
			AssertEquals("PK", DestinationWrapper.Destination.PK, newDestinationWrapper.Destination.PK);
			AssertEquals("Factories Different", true, DestinationWrapper.Factory != newDestinationWrapper.Factory);
		}

		public void TestMissingATAIsACriticalError()
		{
			AssertEquals("Contains Error", true, ((VoyageDestinationAIRAARManagerForTest)Manager).GetNotificationsForSendingAnOriginal().ContainsError("You can't send this actual arrival report as the ATA has not been filled in."));
			DestinationWrapper.Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			DestinationWrapper.Destination.JB_A_ARV = ZDateTime.Now;
			AssertEquals("Contains Error", false, ((VoyageDestinationAIRAARManagerForTest)Manager).GetNotificationsForSendingAnOriginal().ContainsError("You can't send this actual arrival report as the ATA has not been filled in."));
		}

		protected override void SetStatus(ZString status)
		{
			DestinationWrapper.ActualArrivalStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new VoyageDestinationAIRAARManagerForTest(DestinationWrapper);

		CustomsVoyageDestinationWrapper destinationWrapper;
		CustomsVoyageDestinationWrapper DestinationWrapper
		{
			get
			{
				if (destinationWrapper == null)
				{
					var voyage = Factory.New<JobVoyage>();
					var voyageWrapper = new CustomsJobVoyageWrapper(voyage);
					var destination = voyage.Destinations.AddNew();
					destinationWrapper = new CustomsVoyageDestinationWrapper(voyageWrapper, destination);
				}
				return destinationWrapper;
			}
		}

		sealed class VoyageDestinationAIRAARManagerForTest : VoyageDestinationAIRAARManager
		{
			public VoyageDestinationAIRAARManagerForTest(CustomsVoyageDestinationWrapper destinationWrapper) : base(destinationWrapper)
			{
			}

			internal new BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject) => base.GetBusinessObjectInNewFactory(businessObject);
		}
	}
}
