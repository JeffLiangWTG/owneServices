using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED829Provider))]
	class ED829ProviderTest : InboundDataProviderTestCase<IED829, ED829Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED829Provider(null));
		}
		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestSendingCustomsOffice()
		{
			AssertEquals("DE003302", dataProvider.SendingCustomsOffice);
		}

		public void TestAcceptanceDate()
		{
			AssertEquals(new ZDate(2020, 4, 30), dataProvider.AcceptanceDate);
		}

		public void TestMRN()
		{
			AssertEquals("20DE12365485421158E2", dataProvider.MRN);
		}

		public void TestExciseMovementEads()
		{
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new[]
			{
				new ED829CBodyNotificationOfAcceptedExportExciseMovementEad(),
				new ED829CBodyNotificationOfAcceptedExportExciseMovementEad()
			};

			CombineAssertions(() =>
			{
				var exciseMovementEads = dataProvider.ExciseMovementEads;
				AssertEquals("Count", 2, exciseMovementEads.Count);
				AssertSame("Cached", exciseMovementEads, dataProvider.ExciseMovementEads);
			});
		}

		public void TestEventProviderConstructor()
		{
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new ED829CBodyNotificationOfAcceptedExportExciseMovementEad[]
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.ExciseMovementEads);
		}

		public void TestEventProviderValues()
		{
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new[]
			{
				new ED829CBodyNotificationOfAcceptedExportExciseMovementEad { AdministrativeReferenceCode = "20DE41000000001870745", SequenceNumber = "1" }
			};

			CombineAssertions(() =>
			{
				var exciseMovementEad = dataProvider.ExciseMovementEads.Single();
				AssertEquals("Ead", "20DE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED829C
			{
				Header = new ED829CHeader
				{
					MessageGroup = ED829CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED829CBody
				{
					NotificationOfAcceptedExport = new ED829CBodyNotificationOfAcceptedExport
					{
						ExportAcceptance = new ED829CBodyNotificationOfAcceptedExportExportAcceptance
						{
							DateOfAcceptance = new DateTime(2020, 4, 30),
							ReferenceNumberOfSenderCustomsOffice = "DE003302",
							DocumentReferenceNumber = "20DE12365485421158E2"
						}
					}
				}
			};
			dataProvider = new ED829Provider(message);
		}
		ED829C message;
		IED829 dataProvider;

		protected override ED829Provider GetProvider()
		{
			message.Body.NotificationOfAcceptedExport.ExciseMovementEad = new[]
			{
				new ED829CBodyNotificationOfAcceptedExportExciseMovementEad(),
				new ED829CBodyNotificationOfAcceptedExportExciseMovementEad()
			};
			return (ED829Provider)dataProvider;
		}
	}
}
