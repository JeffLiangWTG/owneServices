using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE704;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	class IE704ProviderTest : Business.Testing.DataProviderTestCase<IE704Provider>
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
				AssertEquals("Error Type", "15", error.ErrorType);
				AssertEquals("Error Reason", "reason", error.ErrorReason);
			});
		}

		protected override IE704Provider GetProvider() => DataProvider;

		IE704Provider DataProvider
		{
			get
			{
				if (dataProvider == null)
				{
					dataProvider = new IE704Provider(new Ie704Type
					{
						Header = new HeaderType
						{
							MessageSender = "NDEA.IE",
							MessageRecipient = "NDEA.IE",
							DateOfPreparation = new DateTime(2022, 07, 14),
							TimeOfPreparation = "13:29:08",
							MessageIdentifier = "f4259e39-c0cc-4304-8627-8a1fb056130a",
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
								FunctionalError = new System.Collections.ObjectModel.Collection<FunctionalErrorType>(new[]
								{
									new FunctionalErrorType { ErrorLocation = "location", ErrorReason = "reason", ErrorType = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.FunctionalErrorCodes.Item15, OriginalAttributeValue = "original value" },
								}),
							}
						}
					});
				}

				return dataProvider;
			}
		}
		IE704Provider dataProvider;
	}
}
