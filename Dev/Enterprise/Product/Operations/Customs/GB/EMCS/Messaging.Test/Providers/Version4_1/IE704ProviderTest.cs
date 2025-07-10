using System;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.emcsukcodes;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie704uk;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE704ProviderTest : Business.Testing.DataProviderTestCase<IE704Provider>
	{
		public void TestMrnNumber()
		{
			AssertEquals("MRN98761234", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("5", Provider.MrnNumberSequenceNumber);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("MRN98761234", Provider.AdministrativeReferenceCode);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("B000222547896254786321", Provider.LocalReferenceNumber);
		}

		public void TestErrors()
		{
			CombineAssertions(() =>
			{
				var error = Provider.Errors.Single();
				AssertEquals("Error Location", "location", error.ErrorLocation);
				AssertEquals("OriginalAttributeValue", "original value", error.OriginalAttributeValue);
				AssertEquals("Error Type", "4401", error.ErrorType);
				AssertEquals("Error Reason", "reason", error.ErrorReason);
			});
		}

		public void TestActualMessage()
		{
			sampleFile = "Version4_1.TestFiles.IE704.xml";
			GetProvider();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(Provider.MrnNumber), "00AA00000000000000009", Provider.MrnNumber);
				AssertEquals(nameof(Provider.MrnNumberSequenceNumber), ZString.Empty, Provider.MrnNumberSequenceNumber);
				AssertEquals(nameof(Provider.AdministrativeReferenceCode), "00AA00000000000000009", Provider.AdministrativeReferenceCode);
				AssertEquals(nameof(Provider.LocalReferenceNumber), ZString.Empty, Provider.LocalReferenceNumber);
				var error = Provider.Errors.Single();
				AssertEquals(nameof(error.ErrorLocation), ZString.Empty, error.ErrorLocation);
				AssertEquals(nameof(error.OriginalAttributeValue), ZString.Empty, error.OriginalAttributeValue);
				AssertEquals(nameof(error.ErrorType), "4401", error.ErrorType);
				AssertEquals(nameof(error.ErrorReason), "token", error.ErrorReason);
			});
		}

		protected override IE704Provider GetProvider()
		{
			if (!sampleFile.IsEmpty)
			{
				using (var messageStream = Messaging.Testing.EmbeddedResourceHelper.GetdMessageXmlStream(sampleFile))
				{
					message = EMCSXmlObjectSerializer.Deserialize<Ie704Type>(messageStream);
				}
			}
			else
			{
				message = new Ie704Type
				{
					Header = new HeaderType
					{
						MessageSender = "NDEA.GB",
						MessageRecipient = "NDEA.GB",
						DateOfPreparation = new DateTime(2022, 07, 14),
						TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
						MessageIdentifier = "D8BF12A3-46A7-462D-B465-465A06CDEC4B",
						CorrelationIdentifier = "00000000000215",
					},
					Body = new BodyType
					{
						GenericRefusalMessage = new GenericRefusalMessageType
						{
							Attributes = new AttributesType
							{
								AdministrativeReferenceCode = "MRN98761234",
								LocalReferenceNumber = "B000222547896254786321",
								SequenceNumber = "5",
							},
							FunctionalError = new System.Collections.ObjectModel.Collection<FunctionalErrorType>
								{
									new FunctionalErrorType { ErrorLocation = "location", ErrorReason = "reason", ErrorType = FunctionalErrorCodes.Item4401, OriginalAttributeValue = "original value" },
								},
						}
					}
				};
			}
			return new IE704Provider(message);
		}
		Ie704Type message;

		ZString sampleFile = ZString.Empty;
	}
}
