using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED807Provider))]
	class ED807ProviderTest : InboundDataProviderTestCase<IED807, ED807Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED807Provider(null));
		}

		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestExciseMovementEad()
		{
			message.Body.InterruptionOfMovement.Attributes = new ED807BBodyInterruptionOfMovementAttributes
			{
				AdministrativeReferenceCode = "20DE41000000001870745",
				ReasonForInterruptionCode = "1",
				ComplementaryInformation = "The movement has been interrupted"
			};

			CombineAssertions(() =>
			{
				var exciseMovementEad = dataProvider.ExciseMovementEad;
				AssertEquals("Ead", "20DE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
				AssertEquals("Reason", "1", exciseMovementEad.Reason);
				AssertEquals("ComplementaryInformation", "The movement has been interrupted", exciseMovementEad.ComplementaryInformation);
				AssertSame("Cached", exciseMovementEad, dataProvider.ExciseMovementEad);
			});
		}

		public void TestControlReportNumbers()
		{
			message.Body.InterruptionOfMovement.ReferenceControlReport = new[]
			{
				new ED807BBodyInterruptionOfMovementReferenceControlReport
				{
					ControlReportReference = "46332156"
				},
				new ED807BBodyInterruptionOfMovementReferenceControlReport
				{
					ControlReportReference = "64831215"
				}
			};

			CombineAssertions(() =>
			{
				var numbers = dataProvider.ControlReportNumbers;
				AssertContainsExactElementsInAnyOrder("Items", new[] { "46332156", "64831215" }, numbers);
				AssertSame("Cached", numbers, dataProvider.ControlReportNumbers);
			});
		}

		public void TestControlReportNumbers_Empty()
		{
			CombineAssertions(() =>
			{
				var numbers = dataProvider.ControlReportNumbers;
				AssertEquals("Empty", 0, numbers.Count);
				AssertSame("Cached", numbers, dataProvider.ControlReportNumbers);
			});
		}

		public void TestEventReportNumbers()
		{
			message.Body.InterruptionOfMovement.ReferenceEventReport = new[]
			{
				new ED807BBodyInterruptionOfMovementReferenceEventReport
				{
					EventReportNumber = "64533189"
				},
				new ED807BBodyInterruptionOfMovementReferenceEventReport
				{
					EventReportNumber = "2036454D"
				}
			};

			CombineAssertions(() =>
			{
				var numbers = dataProvider.EventReportNumbers;
				AssertContainsExactElementsInAnyOrder("Items", new[] { "64533189", "2036454D" }, numbers);
				AssertSame("Cached", numbers, dataProvider.EventReportNumbers);
			});
		}

		public void TestEventReportNumbers_Empty()
		{
			CombineAssertions(() =>
			{
				var numbers = dataProvider.EventReportNumbers;
				AssertEquals("Empty", 0, numbers.Count);
				AssertSame("Cached", numbers, dataProvider.EventReportNumbers);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED807B
			{
				Header = new ED807BHeader
				{
					MessageGroup = ED807BHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED807BBody
				{
					InterruptionOfMovement = new ED807BBodyInterruptionOfMovement()
				}
			};
			dataProvider = new ED807Provider(message);
		}
		ED807B message;
		IED807 dataProvider;

		protected override ED807Provider GetProvider() => (ED807Provider)dataProvider;
	}
}
