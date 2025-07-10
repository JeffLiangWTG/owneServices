using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE802;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE802ProviderTest : Business.Testing.DataProviderTestCase<IE802Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE802Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20DE41000000001870745", Provider.MrnNumber);
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
				AssertEquals("Ead", "20DE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
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

		public void TestDateAndTimeOfIssuanceOfReminder()
		{
			AssertEquals("DateAndTimeOfIssuanceOfReminder", new DateTime(2022, 9, 3), Provider.DateAndTimeOfIssuanceOfReminder);
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
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 09, 01),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType()
				{
					ReminderMessageForExciseMovement = new ReminderMessageForExciseMovementType()
					{
						ExciseMovement = new ExciseMovementType
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						},
						Attributes = new AttributesType
						{
							LimitDateAndTime = new DateTime(2020, 5, 8),
							DateAndTimeOfIssuanceOfReminder = new DateTime(2022, 9, 3),
							ReminderInformation = new LsdReminderInformationType
							{
								Value = "ReminderInformation",
								Language = "en"
							},
							ReminderMessageType = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.ReminderMessageType.Item1
						}
					}
				}
			};
			return new IE802Provider(message);
		}
		Ie802Type message;
	}
}
