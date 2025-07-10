using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
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
			message.Body.ReminderMessageForExciseMovement.ExciseMovementEad = new ED802BBodyReminderMessageForExciseMovementExciseMovementEad() { AdministrativeReferenceCode = "20DE41000000001870745", SequenceNumber = "1" };

			CombineAssertions(() =>
			{
				var exciseMovementEad = dataProvider.ExciseMovement;
				AssertEquals("Ead", "20DE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
				AssertSame("Cached", exciseMovementEad, dataProvider.ExciseMovement);
			});
		}

		public void TestLimitDateTime()
		{
			message.Body.ReminderMessageForExciseMovement.Attributes = new ED802BBodyReminderMessageForExciseMovementAttributes() { LimitDateAndTime = new DateTime(2020, 5, 8) };
			AssertEquals("LimitDateTime", new ZDateTime(2020, 5, 8), dataProvider.LimitDateTime);
		}

		public void TestReminderInformation()
		{
			message.Body.ReminderMessageForExciseMovement.Attributes = new ED802BBodyReminderMessageForExciseMovementAttributes() { ReminderInformation = "ReminderInformation" };
			AssertEquals("ReminderInformation", "ReminderInformation", dataProvider.ReminderInformation);
		}

		public void TestReminderMessageType()
		{
			message.Body.ReminderMessageForExciseMovement.Attributes = new ED802BBodyReminderMessageForExciseMovementAttributes() { ReminderMessageType = ED802BBodyReminderMessageForExciseMovementAttributesReminderMessageType.Item1 };
			AssertEquals("ReminderMessageType", "1", dataProvider.ReminderMessageType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED802B()
			{
				Header = new ED802BHeader()
				{
					MessageGroup = ED802BHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED802BBody()
				{
					ReminderMessageForExciseMovement = new ED802BBodyReminderMessageForExciseMovement()
				}
			};
			dataProvider = new ED802Provider(message);
		}
		ED802B message;
		IED802 dataProvider;

		protected override ED802Provider GetProvider() => (ED802Provider)dataProvider;
	}
}
