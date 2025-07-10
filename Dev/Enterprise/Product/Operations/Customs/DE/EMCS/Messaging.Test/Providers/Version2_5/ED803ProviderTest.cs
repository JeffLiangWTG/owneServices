using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED803Provider))]
	class ED803ProviderTest : InboundDataProviderTestCase<IED803, ED803Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED803Provider(null));
		}
		public void TestMessageGroup()
		{
			AssertEquals("EMA", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestExciseMovementEad()
		{
			message.Body.NotificationOfDivertedEad.ExciseNotification = new ED803BBodyNotificationOfDivertedEadExciseNotification { AdministrativeReferenceCode = "20DE41000000001870745", SequenceNumber = "1" };

			CombineAssertions(() =>
			{
				var exciseMovementEad = dataProvider.ExciseMovementEad;
				AssertEquals("Ead", "20DE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
				AssertSame("Cached", exciseMovementEad, dataProvider.ExciseMovementEad);
			});
		}

		public void TestNotificationDateTime()
		{
			AssertEquals("NotificationDateTime", new ZDateTime(2020, 5, 11, 10, 59, 59), dataProvider.NotificationDateTime);
		}

		public void TestNotificationType()
		{
			AssertEquals("NotificationType", "1", dataProvider.NotificationType);
		}

		public void TestDownstreamARCs()
		{
			CombineAssertions(() =>
			{
				message.Body.NotificationOfDivertedEad.DownstreamArc = new[]
				{
						new ED803BBodyNotificationOfDivertedEadDownstreamArc
						{
							AdministrativeReferenceCode = "20DE66421598431563461"
						}
				};
				dataProvider = new ED803Provider(message);
				var downstreamArcs = dataProvider.DownstreamARCs;
				AssertEquals("dataProvider.DownstreamARCs Count", 1, downstreamArcs.Count);
				AssertEquals("DownstreamARC", "20DE66421598431563461", downstreamArcs.First());
				AssertSame("Cached", downstreamArcs, dataProvider.DownstreamARCs);
			});
		}

		public void TestDownstreamARCs_Empty()
		{
			AssertEquals("dataProvider.DownstreamARCs Count", 0, dataProvider.DownstreamARCs.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED803B
			{
				Header = new ED803BHeader
				{
					MessageGroup = ED803BHeaderMessageGroup.EMA,
					MessageIdentifier = "0072260102"
				},
				Body = new ED803BBody
				{
					NotificationOfDivertedEad = new ED803BBodyNotificationOfDivertedEad
					{
						ExciseNotification = new ED803BBodyNotificationOfDivertedEadExciseNotification
						{
							NotificationDateAndTime = new DateTime(2020, 5, 11, 10, 59, 59),
							NotificationType = ED803BBodyNotificationOfDivertedEadExciseNotificationNotificationType.Item1
						}
					}
				}
			};
			dataProvider = new ED803Provider(message);
		}
		ED803B message;
		IED803 dataProvider;

		protected override ED803Provider GetProvider() => (ED803Provider)dataProvider;
	}
}
