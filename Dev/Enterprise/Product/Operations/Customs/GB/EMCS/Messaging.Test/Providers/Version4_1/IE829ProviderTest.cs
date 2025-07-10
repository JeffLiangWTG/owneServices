using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie829;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE829ProviderTest : Business.Testing.DataProviderTestCase<IE829Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE829Provider(null));
		}

		public void TestSendingCustomsOffice()
		{
			AssertEquals("GB003302", Provider.SendingCustomsOffice);
		}

		public void TestAcceptanceDate()
		{
			AssertEquals(new ZDate(2022, 8, 24), Provider.AcceptanceDate);
		}

		public void TestMrn()
		{
			AssertEquals("20GB12365485421158E2", Provider.Mrn);
		}

		public void TestMrnNumber()
		{
			AssertEquals("20GB41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestExciseMovementEads()
		{
			var provider = Provider;
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new System.Collections.ObjectModel.Collection<ExciseMovementEadType>
			{
				new ExciseMovementEadType(),
				new ExciseMovementEadType()
			};

			CombineAssertions(() =>
			{
				var exciseMovementEads = provider.ExciseMovementEads;
				AssertEquals("Count", 2, exciseMovementEads.Count);
				AssertSame("Cached", exciseMovementEads, provider.ExciseMovementEads);
			});
		}

		public void TestEventProviderConstructor()
		{
			var provider = Provider;
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new System.Collections.ObjectModel.Collection<ExciseMovementEadType>
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = provider.ExciseMovementEads);
		}

		public void TestEventProviderValues()
		{
			var provider = Provider;
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new System.Collections.ObjectModel.Collection<ExciseMovementEadType>
			{
				new ExciseMovementEadType { AdministrativeReferenceCode = "20GB41000000001870745", SequenceNumber = "1" }
			};

			CombineAssertions(() =>
			{
				var exciseMovementEad = provider.ExciseMovementEads.Single();
				AssertEquals("Ead", "20GB41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
			});
		}

		protected override IEnumerable<Expression<Func<IE829Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ExciseMovementEads;
		}

		protected override IE829Provider GetProvider()
		{
			message = new Ie829Type
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
					NotificationOfAcceptedExport = new NotificationOfAcceptedExportType
					{
						ExportDeclarationAcceptanceRelease = new ExportDeclarationAcceptanceReleaseType
						{
							DateOfAcceptance = new DateTime(2022, 8, 24),
							ReferenceNumberOfSenderCustomsOffice = "GB003302",
							DocumentReferenceNumber = "20GB12365485421158E2",
						},
						ExciseMovementEad = new System.Collections.ObjectModel.Collection<ExciseMovementEadType>
						{
							new ExciseMovementEadType
							{
								AdministrativeReferenceCode = "20GB41000000001870745",
								SequenceNumber = "1"
							}
						}
					}
				}
			};
			return new IE829Provider(message);
		}

		Ie829Type message;
	}
}
