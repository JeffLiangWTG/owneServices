using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE839;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE839ProviderTest : Business.Testing.DataProviderTestCase<IE839Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE839Provider(null));
		}

		public void TestSendingCustomsOffice()
		{
			AssertEquals("NDEA.IE", Provider.SendingCustomsOffice);
		}

		public void TestIssuanceDate()
		{
			AssertEquals(new ZDate(2022, 8, 24), Provider.IssuanceDate);
		}

		public void TestMRN()
		{
			AssertEquals("20IE12365485421158E2", Provider.MRN);
		}

		public void TestMrnNumber()
		{
			AssertEquals("20IE41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestRejectionReasonCode()
		{
			AssertNotNull("RejectionReasonCode", Provider.RejectionReasonCode);
		}

		public void TestRejectedEads()
		{
			AssertNotNull("RejectedEads", Provider.RejectedEads);
		}

		protected override IE839Provider GetProvider()
		{
			message = new Ie839Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType
				{
					RefusalByCustoms = new RefusalByCustomsType
					{
						Attributes = new AttributesType
						{
							DateAndTimeOfIssuance = new DateTime(2022, 8, 24)
						},
						ExportDeclarationInformation = new ExportDeclarationInformationType
						{
							DocumentReferenceNumber = "20IE12365485421158E2"
						},
						Rejection = new RejectionType
						{
							RejectionDateAndTime = new DateTime(2022, 8, 24),
							RejectionReasonCode = new CustomsRejectionReasonCode(),
						},
						CEadVal = new Collection<CEadValType>
						{
							new CEadValType
							{
								AdministrativeReferenceCode = "20IE41000000001870745",
								SequenceNumber = "1"
							}
						}
					}
				}
			};

			return new IE839Provider(message);
		}

		Ie839Type message;
	}
}
