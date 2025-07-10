using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie905;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE905ProviderTest : Business.Testing.DataProviderTestCase<IE905Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE905Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals(nameof(Provider.MrnNumber), "MRN1234567", Provider.MrnNumber);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals(nameof(Provider.AdministrativeReferenceCode), "MRN1234567", Provider.AdministrativeReferenceCode);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals(nameof(Provider.MrnNumberSequenceNumber), "5", Provider.MrnNumberSequenceNumber);
		}

		public void TestLastReceivedMessageType()
		{
			AssertEquals(nameof(Provider.LastReceivedMessageType), "IE801", Provider.LastReceivedMessageType);
		}

		public void TestStatus()
		{
			AssertEquals(nameof(Provider.Status), "X06", Provider.Status);
		}

		protected override IE905Provider GetProvider()
		{
			message = new Ie905Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
					MessageIdentifier = "D4EC48DB-14A0-46A2-941A-87D0A26E976D",
					CorrelationIdentifier = "00000000000215",
				},
				Body = new BodyType
				{
					StatusResponse = new StatusResponseType
					{
						Attributes = new AttributesType
						{
							AdministrativeReferenceCode = "MRN1234567",
							LastReceivedMessageType = RequestedMessageType.Ie801,
							SequenceNumber = "5",
							Status = StatusType.X06
						}
					}
				}
			};
			return new IE905Provider(message);
		}

		Ie905Type message;
	}
}
