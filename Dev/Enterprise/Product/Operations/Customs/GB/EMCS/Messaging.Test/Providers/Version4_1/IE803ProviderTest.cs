using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie803;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE803ProviderTest : Business.Testing.DataProviderTestCase<IE803Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE803Provider(null));
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

		public void TestNotificationDateTime()
		{
			AssertEquals("NotificationDateTime", new ZDateTime(2022, 09, 01, 15, 30, 05), Provider.NotificationDateTime);
		}

		public void TestNotificationType()
		{
			AssertEquals("NotificationType", "1", Provider.NotificationType);
		}

		public void TestDownstreamARCs()
		{
			CombineAssertions(() =>
			{
				var provider = Provider;
				message.Body.NotificationOfDivertedEadesad.DownstreamArc = new System.Collections.ObjectModel.Collection<DownstreamArcType>()
				{
					new DownstreamArcType
					{
						AdministrativeReferenceCode = "20GB66421598431563461"
					}
				};
				provider = new IE803Provider(message);
				AssertEquals("dataProvider.DownstreamARCs Count", 1, provider.DownstreamARCs.Count);
				AssertEquals("DownstreamARC", "20GB66421598431563461", provider.DownstreamARCs.First());
			});
		}

		public void TestDownstreamARCs_Empty()
		{
			AssertEquals("dataProvider.DownstreamARCs Count", 0, Provider.DownstreamARCs.Count);
		}

		protected override IEnumerable<Expression<Func<IE803Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ExciseMovementEad;
			yield return x => x.DownstreamARCs;
		}

		protected override IE803Provider GetProvider()
		{
			message = new Ie803Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = new DateTime(2022, 08, 24, 15, 30, 08, 000),
					MessageIdentifier = "68097009-AF06-436E-A6DC-5E09F183CF27",
				},
				Body = new BodyType
				{
					NotificationOfDivertedEadesad = new NotificationOfDivertedEadesadType
					{
						ExciseNotification = new ExciseNotificationType
						{
							NotificationDateAndTime = new DateTime(2022, 09, 01, 15, 30, 05),
							NotificationType = NotificationType.Item1,
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						}
					}
				}
			};
			return new IE803Provider(message);
		}
		Ie803Type message;
	}
}
