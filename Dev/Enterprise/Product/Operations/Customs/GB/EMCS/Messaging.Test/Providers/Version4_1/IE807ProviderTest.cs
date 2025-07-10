using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie807;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE807ProviderTest : DataProviderTestCase<IE807Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE807Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20GB41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", Provider.MessageIdentifier);
		}

		public void TestExciseMovementEad()
		{
			CombineAssertions(() =>
			{
				var exciseMovementEad = Provider.ExciseMovementEad;
				AssertEquals("Ead", "20GB41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
				AssertEquals("Reason", "1", exciseMovementEad.Reason);
				AssertEquals("ComplementaryInformation", "The movement has been interrupted", exciseMovementEad.ComplementaryInformation);
				AssertSame("Cached", exciseMovementEad, Provider.ExciseMovementEad);
			});
		}

		public void TestControlReportNumbers()
		{
			CombineAssertions(() =>
			{
				var numbers = Provider.ControlReportNumbers;
				AssertContainsExactElementsInAnyOrder("Items", new[] { "46332156", "64831215" }, numbers);
				AssertSame("Cached", numbers, Provider.ControlReportNumbers);
			});
		}

		public void TestControlReportNumbers_Empty()
		{
			CombineAssertions(() =>
			{
				var numbers = Provider.ControlReportNumbers;
				AssertEquals("ControlReportNumbers", 2, numbers.Count);
				AssertSame("Cached", numbers, Provider.ControlReportNumbers);
			});
		}

		public void TestEventReportNumbers()
		{
			CombineAssertions(() =>
			{
				var numbers = Provider.EventReportNumbers;
				AssertContainsExactElementsInAnyOrder("Items", new[] { "64533189", "2036454D" }, numbers);
				AssertSame("Cached", numbers, Provider.EventReportNumbers);
			});
		}

		public void TestEventReportNumbers_Empty()
		{
			CombineAssertions(() =>
			{
				var numbers = Provider.EventReportNumbers;
				AssertEquals("EventReportNumbers", 2, numbers.Count);
				AssertSame("Cached", numbers, Provider.EventReportNumbers);
			});
		}

		protected override IE807Provider GetProvider()
		{
			message = new Ie807Type
			{
				Header = new HeaderType
				{
					MessageIdentifier = "0072260102"
				},
				Body = new BodyType
				{
					InterruptionOfMovement = new InterruptionOfMovementType()
					{
						Attributes = new AttributesType
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							ReasonForInterruptionCode = "1",
							ComplementaryInformation = new LsdComplementaryInformationType() { Language = "en", Value = "The movement has been interrupted" }
						},
						ReferenceControlReport = new Collection<ReferenceControlReportType>
						{
							new ReferenceControlReportType
							{
								ControlReportReference = "46332156"
							},
							new ReferenceControlReportType
							{
								ControlReportReference = "64831215"
							}
						},
						ReferenceEventReport = new Collection<ReferenceEventReportType>
						{
							new ReferenceEventReportType
							{
								EventReportNumber = "64533189"
							},
							new ReferenceEventReportType
							{
								EventReportNumber = "2036454D"
							}
						}
					}
				}
			};
			return new IE807Provider(message);
		}
		Ie807Type message;
	}
}
