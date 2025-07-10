using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie840;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE840ProviderTest : Business.Testing.DataProviderTestCase<IE840Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE840Provider(null));
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
			AssertNotNull(Provider.ExciseMovementEad);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("Ead", "20GB41000000001870745", Provider.ExciseMovementEad.AdministrativeReferenceCode);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence", "1", Provider.ExciseMovementEad.SequenceNumber);
		}

		protected override IE840Provider GetProvider()
		{
			message = new Ie840Type()
			{
				Header = new HeaderType
				{
					MessageIdentifier = "0072260102"
				},
				Body = new BodyType()
				{
					EventReportEnvelope = new EventReportEnvelopeType()
					{
						ExciseMovement = new ExciseMovementType()
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						}
					}
				}
			};
			return new IE840Provider(message);
		}
		Ie840Type message;
	}
}
