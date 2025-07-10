using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondAIROUTAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierChangesIfFlightChanges()
		{
			var underbond = (CusUnderbond)GetSavedBizo();
			AssertEquals(false, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
			CTOMAWB.CM_FlightNo = "QF1232";
			AssertEquals(true, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
		}

		public void TestUniqueIdentifierChangesIfEstimatedArrivalDateChanges()
		{
			var underbond = (CusUnderbond)GetSavedBizo();
			AssertEquals(false, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
			CTOMAWB.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(true, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
		}

		public void TestUniqueIdentifierChangesIfOutturnEstablishmentChanges()
		{
			var underbond = (CusUnderbond)GetSavedBizo();
			AssertEquals(false, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
			underbond.C4_DestinationPremiseID = "32232";
			AssertEquals(true, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
		}

		public void TestUniqueIdentifierChangesIfDateTimeOfOutturnChanges()
		{
			var underbond = (CusUnderbond)GetSavedBizo();
			AssertEquals(false, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
			underbond.C4_Outurned = ZDateTime.Now;
			AssertEquals(true, new CusUnderbondAIROUTAmendmentGeneratorForTest(underbond).UniqueIdentifierBeingChanged);
		}

		public void TestStandAloneOutturnCanStillDoAmendment()
		{
			var standAloneUnderbond = Factory.New<CusUnderbond>();
			standAloneUnderbond.C4_Outurned = ZDateTime.Today;
			standAloneUnderbond.C4_MAWB = "081623423";
			var outturnLine = standAloneUnderbond.Outturns.AddNew();
			outturnLine.C5_HouseBill = "HOUSE";
			outturnLine.C5_PackagesOutturned = 5;
			var message = Factory.New<CMRAIROUTMessage>();
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			standAloneUnderbond.Messages.Add(message);
			Factory.Save();
			var generator = new CusUnderbondAIROUTAmendmentGenerator(standAloneUnderbond);
			var messageBuilder = generator.GetBuilder(standAloneUnderbond);
			messageBuilder.MessageSubType = Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Change;
			messageBuilder.Messages = generator.GetMesssageCollection(Underbond);
			AssertNotNull(messageBuilder);
			string messageResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+263:::AIROUT+<<SENDERS REFERENCE PLACE HOLDER>>";
			Assert("Message text should be equal", messageBuilder.MessageText.Contains(messageResult));
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusUnderbondAIROUTAmendmentGeneratorForTest(bizo as CusUnderbond);

		protected override Type ExpectedMessageType => typeof(CMRAIROUTMessage);

		protected override void AssertCommon(EDIMessage message)
		{
			AssertEquals("MessageType", ExpectedMessageType, message.GetType());
			AssertEquals("EM_LinkedObject", typeof(CusUnderbond), message.EM_LinkedObject.GetType());
		}

		protected override BusinessObject GetSavedBizo()
		{
			Underbond.Factory.Save();
			return Underbond;
		}

		CusUnderbond underbond;
		CusUnderbond Underbond
		{
			get
			{
				if (underbond == null)
				{
					underbond = Factory.New<CusUnderbond>();
					underbond.LinkedObject = CTOMAWB;
				}
				return underbond;
			}
		}

		CTOCusMAWB ctoMAWB;
		CTOCusMAWB CTOMAWB => ctoMAWB ?? (ctoMAWB = Factory.New<CTOCusMAWB>());

		sealed class CusUnderbondAIROUTAmendmentGeneratorForTest : CusUnderbondAIROUTAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusUnderbondAIROUTAmendmentGeneratorForTest(CusUnderbond underbond) : base(underbond)
			{
			}

			internal new bool UniqueIdentifierBeingChanged => base.UniqueIdentifierBeingChanged;
			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
