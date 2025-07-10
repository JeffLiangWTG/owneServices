using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie802;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE802ProviderTest : Business.Testing.DataProviderTestCase<IE802Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE802Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20GB41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestExciseMovementEad()
		{
			CombineAssertions(() =>
			{
				var exciseMovementEad = Provider.ExciseMovementEad;
				AssertEquals("Ead", "20GB41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
			});
		}

		public void TestLimitDateTime()
		{
			AssertEquals("LimitDateTime", new DateTime(2020, 5, 8), Provider.LimitDateTime);
		}

		public void TestReminderInformation()
		{
			AssertEquals("ReminderInformation", "ReminderInformation", Provider.ReminderInformation);
		}

		public void TestReminderMessageType()
		{
			AssertEquals("ReminderMessageType", "1", Provider.ReminderMessageType);
		}

		protected override IEnumerable<Expression<Func<IE802Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ExciseMovementEad;
		}

		protected override IE802Provider GetProvider()
		{
			message = new Ie802Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 09, 01),
					TimeOfPreparation = new DateTime(2022, 09, 01, 15, 30, 05),
					MessageIdentifier = "A0AF269A-83CF-451C-8D31-1964316649D1",
				},
				Body = new BodyType()
				{
					ReminderMessageForExciseMovement = new ReminderMessageForExciseMovementType()
					{
						ExciseMovement = new ExciseMovementType
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						},
						Attributes = new AttributesType
						{
							LimitDateAndTime = new DateTime(2020, 5, 8),
							ReminderInformation = new LsdReminderInformationType
							{
								Value = "ReminderInformation",
								Language = "en"
							},
							ReminderMessageType = ReminderMessageType.Item1
						}
					}
				}
			};
			return new IE802Provider(message);
		}
		Ie802Type message;
	}
}
