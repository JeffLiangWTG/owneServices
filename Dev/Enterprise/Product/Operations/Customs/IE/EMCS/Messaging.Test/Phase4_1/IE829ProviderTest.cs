using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE829;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE829ProviderTest : Business.Testing.DataProviderTestCase<IE829Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE829Provider(null));
		}

		public void TestSendingCustomsOffice()
		{
			AssertEquals("IE003302", Provider.SendingCustomsOffice);
		}

		public void TestAcceptanceDate()
		{
			AssertEquals(new ZDate(2022, 8, 24), Provider.AcceptanceDate);
		}

		public void TestReleaseDate()
		{
			AssertEquals(new ZDate(2023, 9, 5), Provider.ReleaseDate);
		}

		public void TestMrn()
		{
			AssertEquals("20IE12365485421158E2", Provider.Mrn);
		}

		public void TestMrnNumber()
		{
			AssertEquals("20IE41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestExciseMovementEads()
		{
			var provider = Provider;
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new Collection<ExciseMovementEadType>
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
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new Collection<ExciseMovementEadType>
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = provider.ExciseMovementEads);
		}

		public void TestEventProviderValues()
		{
			var provider = Provider;
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new Collection<ExciseMovementEadType>
			{
				new ExciseMovementEadType { AdministrativeReferenceCode = "20IE41000000001870745", SequenceNumber = "1" }
			};

			CombineAssertions(() =>
			{
				var exciseMovementEad = provider.ExciseMovementEads.Single();
				AssertEquals("Ead", "20IE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
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
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType
				{
					NotificationOfAcceptedExport = new NotificationOfAcceptedExportType
					{
						ExportDeclarationAcceptanceRelease = new ExportDeclarationAcceptanceReleaseType
						{
							DateOfAcceptance = new DateTime(2022, 8, 24),
							ReferenceNumberOfSenderCustomsOffice = "IE003302",
							DocumentReferenceNumber = "20IE12365485421158E2",
							DateOfRelease = new DateTime(2023, 9, 5)
						},
						ExciseMovementEad = new Collection<ExciseMovementEadType>
						{
							new ExciseMovementEadType
							{
								AdministrativeReferenceCode = "20IE41000000001870745",
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
