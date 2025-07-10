using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie837;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE837ProviderTest : Business.Testing.DataProviderTestCase<IE837Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE837Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals(nameof(Provider.MrnNumber), "MRN98761234", Provider.MrnNumber);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals(nameof(Provider.AdministrativeReferenceCode), "MRN98761234", Provider.AdministrativeReferenceCode);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals(nameof(Provider.MrnNumberSequenceNumber), "5", Provider.MrnNumberSequenceNumber);
		}

		public void TestExciseMovement()
		{
			CombineAssertions(() =>
			{
				var exciseMovement = Provider.ExciseMovement;
				AssertEquals(nameof(exciseMovement.AdministrativeReferenceCode), "MRN98761234", exciseMovement.AdministrativeReferenceCode);
				AssertEquals(nameof(exciseMovement.SequenceNumber), "5", exciseMovement.SequenceNumber);
			});
		}

		public void TestComplementaryInformation()
		{
			AssertEquals(nameof(Provider.ComplementaryInformation), "complementaryInformation", Provider.ComplementaryInformation);
		}

		public void TestDateAndTimeOfValidationOfExplanationOnDelay()
		{
			AssertEquals(nameof(Provider.DateAndTimeOfValidationOfExplanationOnDelay), new DateTime(2022, 08, 14), Provider.DateAndTimeOfValidationOfExplanationOnDelay);
		}

		public void TestExplanationCode()
		{
			AssertEquals(nameof(Provider.ExplanationCode), "ABCD", Provider.ExplanationCode);
		}

		public void TestMessageRole()
		{
			AssertEquals(nameof(Provider.MessageRole), "1", Provider.MessageRole);
		}

		public void TestSubmitterIdentification()
		{
			AssertEquals(nameof(Provider.SubmitterIdentification), "ABCD.EFG", Provider.SubmitterIdentification);
		}

		public void TestSubmitterType()
		{
			AssertEquals(nameof(Provider.SubmitterType), "2", Provider.SubmitterType);
		}

		public void TestActualMessage()
		{
			sampleFile = "Version4_1.TestFiles.IE837.xml";
			GetProvider();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(Provider.MrnNumber), "00AA00000000000000000", Provider.MrnNumber);
				AssertEquals(nameof(Provider.AdministrativeReferenceCode), "00AA00000000000000000", Provider.AdministrativeReferenceCode);
				AssertEquals(nameof(Provider.MrnNumberSequenceNumber), "0", Provider.MrnNumberSequenceNumber);
				var exciseMovement = Provider.ExciseMovement;
				AssertEquals(nameof(exciseMovement.AdministrativeReferenceCode), "00AA00000000000000000", exciseMovement.AdministrativeReferenceCode);
				AssertEquals(nameof(exciseMovement.SequenceNumber), "0", exciseMovement.SequenceNumber);
				AssertEquals(nameof(Provider.ComplementaryInformation), "token", Provider.ComplementaryInformation);
				AssertEquals(nameof(Provider.DateAndTimeOfValidationOfExplanationOnDelay), new DateTime(2001, 12, 17, 9, 30, 47), Provider.DateAndTimeOfValidationOfExplanationOnDelay);
				AssertEquals(nameof(Provider.ExplanationCode), "1", Provider.ExplanationCode);
				AssertEquals(nameof(Provider.MessageRole), "1", Provider.MessageRole);
				AssertEquals(nameof(Provider.SubmitterIdentification), "AA12345678901", Provider.SubmitterIdentification);
				AssertEquals(nameof(Provider.SubmitterType), "1", Provider.SubmitterType);
			});
		}

		protected override IEnumerable<Expression<Func<IE837Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ExciseMovement;
		}

		protected override IE837Provider GetProvider()
		{
			if (!sampleFile.IsEmpty)
			{
				using (var messageStream = Messaging.Testing.EmbeddedResourceHelper.GetdMessageXmlStream(sampleFile))
				{
					message = EMCSXmlObjectSerializer.Deserialize<Ie837Type>(messageStream);
				}
			}
			else
			{
				message = new Ie837Type
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
						ExplanationOnDelayForDelivery = new ExplanationOnDelayForDeliveryType
						{
							Attributes = new AttributesType
							{
								ComplementaryInformation = new LsdComplementaryInformationType
								{
									Language = "en",
									Value = "complementaryInformation"
								},
								DateAndTimeOfValidationOfExplanationOnDelay = new DateTime(2022, 08, 14),
								ExplanationCode = "ABCD",
								MessageRole = MessageRoleCode.Item1,
								SubmitterIdentification = "ABCD.EFG",
								SubmitterType = SubmitterType.Item2
							},
							ExciseMovement = new ExciseMovementType
							{
								AdministrativeReferenceCode = "MRN98761234",
								SequenceNumber = "5"
							}
						}
					}
				};
			}
			return new IE837Provider(message);
		}

		Ie837Type message;

		ZString sampleFile = ZString.Empty;
	}
}
