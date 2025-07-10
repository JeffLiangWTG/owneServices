using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie839;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE839ProviderTest : Business.Testing.DataProviderTestCase<IE839Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE839Provider(null));
		}

		public void TestSendingCustomsOffice()
		{
			AssertEquals(nameof(Provider.SendingCustomsOffice), "NDEA.GB", Provider.SendingCustomsOffice);
		}

		public void TestIssuanceDate()
		{
			AssertEquals(nameof(Provider.IssuanceDate), new ZDate(2022, 8, 24), Provider.IssuanceDate);
		}

		public void TestMRN()
		{
			AssertEquals(nameof(Provider.MRN), "20GB12365485421158E2", Provider.MRN);
		}

		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(Provider.LocalReferenceNumber), ZString.Empty, Provider.LocalReferenceNumber);
				message.Body.RefusalByCustoms.ExportDeclarationInformation = new ExportDeclarationInformationType() { LocalReferenceNumber = "20GB12365489012345E1" };
				AssertEquals(nameof(Provider.LocalReferenceNumber), "20GB12365489012345E1", Provider.LocalReferenceNumber);
			});
		}

		public void TestMrnNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(Provider.MrnNumber), ZString.Empty, Provider.MrnNumber);
				cEadValTypeNumber = 1;
				var provider = GetProvider();
				AssertEquals(nameof(provider.MrnNumber), "20GB41000000001870745", provider.MrnNumber);
			});
		}

		public void TestMrnNumberSequenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(Provider.MrnNumberSequenceNumber), ZString.Empty, Provider.MrnNumberSequenceNumber);
				cEadValTypeNumber = 1;
				var provider = GetProvider();
				AssertEquals(nameof(provider.MrnNumberSequenceNumber), "1", provider.MrnNumberSequenceNumber);
			});
		}

		public void TestRejectionReasonCode()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(nameof(Provider.RejectionReasonCode), Provider.RejectionReasonCode);
				AssertEquals(nameof(Provider.RejectionReasonCode), "1", Provider.RejectionReasonCode);
			});
		}

		public void TestRejectedEads()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(nameof(Provider.RejectedEads), Provider.RejectedEads);
				AssertEquals($"Count {nameof(Provider.RejectedEads)}", 0, Provider.RejectedEads.Count);

				cEadValTypeNumber = 2;
				var provider = GetProvider();
				AssertEquals($"Count {nameof(provider.RejectedEads)}", 2, provider.RejectedEads.Count);
				AssertEquals(nameof(provider.RejectedEads), "20GB41000000001870745", provider.RejectedEads.First().AdministrativeReferenceCode);
			});
		}

		protected override IE839Provider GetProvider()
		{
			message = new Ie839Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = new DateTime(2022, 08, 24, 15, 30, 08, 000),
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
							DocumentReferenceNumber = "20GB12365485421158E2"
						},
						Rejection = new RejectionType
						{
							RejectionDateAndTime = new DateTime(2022, 8, 24),
							RejectionReasonCode = new CustomsRejectionReasonCode(),
						},
						CEadVal = GetCEadVal(cEadValTypeNumber),
					}
				}
			};

			return new IE839Provider(message);
		}
		int cEadValTypeNumber;
		Ie839Type message;

		Collection<CEadValType> GetCEadVal(int elements)
		{
			switch (elements)
			{
				case 1:
					return new Collection<CEadValType> { new CEadValType { AdministrativeReferenceCode = "20GB41000000001870745", SequenceNumber = "1" } };
				case 2:
					return new Collection<CEadValType> { new CEadValType { AdministrativeReferenceCode = "20GB41000000001870745", SequenceNumber = "1" }, new CEadValType { AdministrativeReferenceCode = "20GB41000000001870750", SequenceNumber = "2" } };
				default:
					return new Collection<CEadValType>();
			}
		}
	}
}
