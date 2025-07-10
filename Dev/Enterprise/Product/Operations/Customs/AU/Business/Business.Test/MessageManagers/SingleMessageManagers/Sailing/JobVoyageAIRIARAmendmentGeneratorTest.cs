using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobVoyageAIRIARAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierChangedByFlight()
		{
			var wrapper = (CustomsJobVoyageWrapper)GetSavedBizo();
			wrapper.Voyage.JV_VoyageFlight = "123";
			AssertEquals(true, GetAIRIARGenerator(wrapper).UniqueIdentifierBeingChanged);
			wrapper.Voyage.JV_VoyageFlight = (ZString)wrapper.Voyage.JV_VoyageFlightInfo.OriginalValue;
			AssertEquals(false, GetAIRIARGenerator(wrapper).UniqueIdentifierBeingChanged);
		}

		public void TestUniqueIdentifierChangedByDeparturePort()
		{
			var wrapper = (CustomsJobVoyageWrapper)GetSavedBizo();
			wrapper.Voyage.Origins[0].JA_RL_NKPortOfLoading = "HKHKG";
			AssertEquals(true, GetAIRIARGenerator(wrapper).UniqueIdentifierBeingChanged);
			wrapper.Voyage.Origins[0].JA_RL_NKPortOfLoading = (ZString)wrapper.Voyage.Origins[0].JA_RL_NKPortOfLoadingInfo.OriginalValue;
			AssertEquals(false, GetAIRIARGenerator(wrapper).UniqueIdentifierBeingChanged);
		}

		[TestDate(2012, 10, 31, 13, 10, 20, 0)]
		public void TestUniqueIdentifierChangedByDeparturetTime()
		{
			var wrapper = (CustomsJobVoyageWrapper)GetSavedBizo();
			wrapper.Voyage.Origins[0].JA_A_DEP = ZDateTime.Now.AddHours(1);
			AssertEquals(true, GetAIRIARGenerator(wrapper).UniqueIdentifierBeingChanged);
			wrapper.Voyage.Origins[0].JA_A_DEP = (ZDateTime)wrapper.Voyage.Origins[0].JA_A_DEPInfo.OriginalValue;
			AssertEquals(false, GetAIRIARGenerator(wrapper).UniqueIdentifierBeingChanged);
		}

		public void TestGetBusinessObjectInNewFactory()
		{
			var wrapper = (CustomsJobVoyageWrapper)GetSavedBizo();
			AssertNotNull("GetBusinessObjectInNewFactory", GetAIRIARGenerator(wrapper).GetBusinessObjectInNewFactory(wrapper));
		}

		protected override BusinessObject DBBizo => CustomsJobVoyageWrapper.Load(new BusinessObjectFactory(), ((CustomsJobVoyageWrapper)Bizo).Voyage.PK);

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new JobVoyageAIRIARAmendmentGeneratorForTest(bizo as CustomsJobVoyageWrapper);

		protected override Type ExpectedMessageType => typeof(CMRAIRIARMessage);

		protected override void AssertCommon(EDIMessage message)
		{
			AssertEquals("MessageType", ExpectedMessageType, message.GetType());
			AssertEquals("EM_LinkedObject", ((CustomsJobVoyageWrapper)Bizo).Voyage, message.EM_LinkedObject);
		}

		protected override BusinessObject GetSavedBizo()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_A_DEP = ZDateTime.Now;
			Factory.Save();
			return new CustomsJobVoyageWrapper(voyage);
		}

		JobVoyageAIRIARAmendmentGeneratorForTest GetAIRIARGenerator(BusinessObject bizo) => new JobVoyageAIRIARAmendmentGeneratorForTest(bizo as CustomsJobVoyageWrapper);

		sealed class JobVoyageAIRIARAmendmentGeneratorForTest : JobVoyageAIRIARAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public JobVoyageAIRIARAmendmentGeneratorForTest(CustomsJobVoyageWrapper voyageWrapper) : base(voyageWrapper)
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
