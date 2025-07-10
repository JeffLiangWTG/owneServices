using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE803;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE803ProviderTest : Business.Testing.DataProviderTestCase<IE803Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE803Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20DE41000000001870745", Provider.MrnNumber);
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
				AssertEquals("Ead", "20DE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
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
				message.Body.NotificationOfDivertedEadesad.DownstreamArc = new Collection<DownstreamArcType>
				{
					new DownstreamArcType
					{
						AdministrativeReferenceCode = "20DE66421598431563461"
					}
				};
				provider = new IE803Provider(message);
				AssertEquals("dataProvider.DownstreamARCs Count", 1, provider.DownstreamARCs.Count);
				AssertEquals("DownstreamARC", "20DE66421598431563461", provider.DownstreamARCs.First());
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
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType
				{
					NotificationOfDivertedEadesad = new NotificationOfDivertedEadesadType()
					{
						ExciseNotification = new ExciseNotificationType
						{
							NotificationDateAndTime = new DateTime(2022, 09, 01, 15, 30, 05),
							NotificationType = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.NotificationType.Item1,
							AdministrativeReferenceCode = "20DE41000000001870745",
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
