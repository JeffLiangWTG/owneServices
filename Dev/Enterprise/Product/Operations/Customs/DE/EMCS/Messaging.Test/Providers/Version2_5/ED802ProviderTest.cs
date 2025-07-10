using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED802Provider))]
	class ED802ProviderTest : InboundDataProviderTestCase<IED802, ED802Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED802Provider(null));
		}
		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestExciseMovement()
		{
			message.Body.ReminderMessageForExciseMovement.ExciseMovement = new ED802CBodyReminderMessageForExciseMovementExciseMovement() { AdministrativeReferenceCode = "20DE41000000001870745", SequenceNumber = "1" };

			CombineAssertions(() =>
			{
				var exciseMovement = dataProvider.ExciseMovement;
				AssertEquals("Administrative Reference Code", "20DE41000000001870745", exciseMovement.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovement.SequenceNumber);
				AssertSame("Cached", exciseMovement, dataProvider.ExciseMovement);
			});
		}

		public void TestLimitDateTime()
		{
			message.Body.ReminderMessageForExciseMovement.Attributes = new ED802CBodyReminderMessageForExciseMovementAttributes() { LimitDateAndTime = new DateTime(2020, 5, 8) };
			AssertEquals("LimitDateTime", new ZDateTime(2020, 5, 8), dataProvider.LimitDateTime);
		}

		public void TestReminderInformation()
		{
			message.Body.ReminderMessageForExciseMovement.Attributes = new ED802CBodyReminderMessageForExciseMovementAttributes() { ReminderInformation = "ReminderInformation" };
			AssertEquals("ReminderInformation", "ReminderInformation", dataProvider.ReminderInformation);
		}

		public void TestReminderMessageType()
		{
			message.Body.ReminderMessageForExciseMovement.Attributes = new ED802CBodyReminderMessageForExciseMovementAttributes() { ReminderMessageType = ED802CBodyReminderMessageForExciseMovementAttributesReminderMessageType.Item1 };
			AssertEquals("ReminderMessageType", "1", dataProvider.ReminderMessageType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED802C()
			{
				Header = new ED802CHeader()
				{
					MessageGroup = ED802CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED802CBody()
				{
					ReminderMessageForExciseMovement = new ED802CBodyReminderMessageForExciseMovement()
				}
			};
			dataProvider = new ED802Provider(message);
		}
		ED802C message;
		IED802 dataProvider;

		protected override ED802Provider GetProvider() => (ED802Provider)dataProvider;
	}
}
