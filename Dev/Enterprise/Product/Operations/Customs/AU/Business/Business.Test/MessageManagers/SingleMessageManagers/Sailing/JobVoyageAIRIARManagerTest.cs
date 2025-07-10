using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobVoyageAIRIARManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestBusinessObject()
		{
			AssertEquals(VoyageWrapper, Manager.BusinessObject);
		}

		public void TestGetCommonNotificationsForSending()
		{
			var abn = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			if (abn.IsEmpty)
			{
				abn = "987654321";
			}

			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "";
			var testManager = (JobVoyageAIRIARManager)GetManager();
			var collection = testManager.GetNotificationsForSendingAnOriginal();
			Assert(collection.ContainsError("You have not entered a Responsible Party ID. Please ensure you have filled out the 'Business Reg No' on your company 'Org. Proxy'."));
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = abn;
			collection = testManager.GetNotificationsForSendingAnOriginal();
			Assert(!collection.ContainsError("You have not entered a Responsible Party ID. Please ensure you have filled out the 'Business Reg No' on your company 'Org. Proxy'."));
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Air Impending Arrival Report", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(VoyageWrapper);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRIARMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(VoyageWrapper);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRIARMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(VoyageWrapper);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRIARMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((JobVoyageAIRIARManager)Manager).GetStatus());
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, ((JobVoyageAIRIARManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(JobVoyageImpendingArrivalStatusCalculator), ((JobVoyageAIRIARManager)Manager).StatusCalculators[0].GetType());
		}

		public void TestGetBusinessObjectInNewFactory()
		{
			VoyageWrapper.Factory.Save();
			var newVoyageWrapper = (CustomsJobVoyageWrapper)((JobVoyageAIRIARManagerForTest)GetManager()).GetBusinessObjectInNewFactory(VoyageWrapper);
			AssertEquals("PK", VoyageWrapper.Voyage.PK, newVoyageWrapper.Voyage.PK);
			AssertEquals("Factories Different", true, VoyageWrapper.Factory != newVoyageWrapper.Factory);
		}

		protected override void SetStatus(ZString status)
		{
			VoyageWrapper.ImpendingArrivalStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new JobVoyageAIRIARManagerForTest(VoyageWrapper);

		CustomsJobVoyageWrapper wrapper;
		CustomsJobVoyageWrapper VoyageWrapper => wrapper ?? (wrapper = new CustomsJobVoyageWrapper(Factory.New<JobVoyage>()));

		sealed class JobVoyageAIRIARManagerForTest : JobVoyageAIRIARManager
		{
			public JobVoyageAIRIARManagerForTest(CustomsJobVoyageWrapper voyageWrapper) : base(voyageWrapper)
			{
			}

			internal new BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject) => base.GetBusinessObjectInNewFactory(businessObject);
		}
	}
}
