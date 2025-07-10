using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class VoyageDestinationAIRAARAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierChangedByFlight()
		{
			var wrapper = (CustomsVoyageDestinationWrapper)GetSavedBizo();
			wrapper.Destination.Voyage.JV_VoyageFlight = "123";
			AssertEquals(true, GetAIRAARGenerator(wrapper).UniqueIdentifierBeingChanged);
			wrapper.Destination.Voyage.JV_VoyageFlight = (ZString)wrapper.Destination.Voyage.JV_VoyageFlightInfo.OriginalValue;
			AssertEquals(false, GetAIRAARGenerator(wrapper).UniqueIdentifierBeingChanged);
		}

		public void TestUniqueIdentifierChangedByDeparturePort()
		{
			var wrapper = (CustomsVoyageDestinationWrapper)GetSavedBizo();
			wrapper.Destination.Voyage.Origins[0].JA_RL_NKPortOfLoading = "HKHKG";
			AssertEquals(true, GetAIRAARGenerator(wrapper).UniqueIdentifierBeingChanged);
			wrapper.Destination.Voyage.Origins[0].JA_RL_NKPortOfLoading = (ZString)wrapper.Destination.Voyage.Origins[0].JA_RL_NKPortOfLoadingInfo.OriginalValue;
			AssertEquals(false, GetAIRAARGenerator(wrapper).UniqueIdentifierBeingChanged);
		}

		[TestDate(2012, 10, 31, 13, 10, 20, 0)]
		public void TestUniqueIdentifierChangedByDeparturetTime()
		{
			var wrapper = (CustomsVoyageDestinationWrapper)GetSavedBizo();
			wrapper.Destination.Voyage.Origins[0].JA_A_DEP = ZDateTime.Now.AddHours(1);
			AssertEquals(true, GetAIRAARGenerator(wrapper).UniqueIdentifierBeingChanged);
			wrapper.Destination.Voyage.Origins[0].JA_A_DEP = (ZDateTime)wrapper.Destination.Voyage.Origins[0].JA_A_DEPInfo.OriginalValue;
			AssertEquals(false, GetAIRAARGenerator(wrapper).UniqueIdentifierBeingChanged);
		}

		public void TestGetBusinessObjectInNewFactory()
		{
			var wrapper = (CustomsVoyageDestinationWrapper)GetSavedBizo();
			AssertNotNull("GetBusinessObjectInNewFactory", GetAIRAARGenerator(wrapper).GetBusinessObjectInNewFactory(wrapper));
		}

		protected override BusinessObject DBBizo => CustomsVoyageDestinationWrapper.Load(new BusinessObjectFactory(), ((CustomsVoyageDestinationWrapper)Bizo).Destination.PK);

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new VoyageDestinationAIRAARAmendmentGeneratorForTest(bizo as CustomsVoyageDestinationWrapper);

		protected override Type ExpectedMessageType => typeof(CMRAIRAARMessage);

		protected override void AssertCommon(EDIMessage message)
		{
			AssertEquals("MessageType", ExpectedMessageType, message.GetType());
			AssertEquals("EM_LinkedObject", ((CustomsVoyageDestinationWrapper)Bizo).Destination, message.EM_LinkedObject);
		}

		protected override BusinessObject GetSavedBizo()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_A_DEP = ZDateTime.Now;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUNTL";
			Factory.Save();

			return new CustomsJobVoyageWrapper(voyage).Destinations[0];
		}

		VoyageDestinationAIRAARAmendmentGeneratorForTest GetAIRAARGenerator(BusinessObject bizo) => new VoyageDestinationAIRAARAmendmentGeneratorForTest(bizo as CustomsVoyageDestinationWrapper);

		sealed class VoyageDestinationAIRAARAmendmentGeneratorForTest : VoyageDestinationAIRAARAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public VoyageDestinationAIRAARAmendmentGeneratorForTest(CustomsVoyageDestinationWrapper destinationWrapper) : base(destinationWrapper)
			{
			}

			internal new bool UniqueIdentifierBeingChanged => base.UniqueIdentifierBeingChanged;
			internal new BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject) => base.GetBusinessObjectInNewFactory(businessObject);
			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
