using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie819;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE819ProviderTest : Business.Testing.DataProviderTestCase<IE819Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE819Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20GB41000000001870745", Provider.MrnNumber);
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
				AssertEquals("Ead", "20GB41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
			});
		}

		public void TestAlertOrRejectionReasons()
		{
			var provider = Provider;
			message.Body.AlertOrRejectionOfEadesad.AlertOrRejectionOfEadEsadReason = new System.Collections.ObjectModel.Collection<AlertOrRejectionOfEadEsadReasonType>
			{
				new AlertOrRejectionOfEadEsadReasonType(),
				new AlertOrRejectionOfEadEsadReasonType()
			};

			AssertEquals("Count", 2, provider.AlertOrRejectionReasons.Count);
		}

		public void TestNoAlertOrRejectionReasons()
		{
			AssertEquals(false, Provider.AlertOrRejectionReasons.Any());
		}

		public void TestReasonProviderConstructor()
		{
			var provider = Provider;
			message.Body.AlertOrRejectionOfEadesad.AlertOrRejectionOfEadEsadReason = new System.Collections.ObjectModel.Collection<AlertOrRejectionOfEadEsadReasonType>
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = provider.AlertOrRejectionReasons);
		}

		public void TestReasonProviderValues()
		{
			var provider = Provider;
			message.Body.AlertOrRejectionOfEadesad.AlertOrRejectionOfEadEsadReason = new System.Collections.ObjectModel.Collection<AlertOrRejectionOfEadEsadReasonType>
			{
				new AlertOrRejectionOfEadEsadReasonType()
				{
					AlertOrRejectionOfMovementReasonCode = "1",
					ComplementaryInformation = new LsdComplementaryInformationType
					{
						Language = "en",
						Value = "Additional Information"
					}
				}
			};

			CombineAssertions(() =>
			{
				var eadReason = provider.AlertOrRejectionReasons.Single();
				AssertEquals("Code", "1", eadReason.ReasonCode);
				AssertEquals("Information", "Additional Information", eadReason.ComplementaryInformation);
			});
		}

		protected override IEnumerable<Expression<Func<IE819Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ExciseMovementEad;
			yield return x => x.AlertOrRejectionReasons;
		}

		protected override IE819Provider GetProvider()
		{
			message = new Ie819Type
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
					AlertOrRejectionOfEadesad = new AlertOrRejectionOfEadesadType
					{
						ExciseMovement = new ExciseMovementType
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						}
					}
				}
			};
			return new IE819Provider(message);
		}
		Ie819Type message;
	}
}
