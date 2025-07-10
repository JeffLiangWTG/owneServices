using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED840Provider))]
	class ED840ProviderTest : InboundDataProviderTestCase<IED840, ED840Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED840Provider(null));
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestMessageGroup()
		{
			AssertEquals(EmcsMessageSubTypeList.Codes.Eme, dataProvider.MessageGroup);
		}

		public void TestExciseMovement()
		{
			AssertNotNull(dataProvider.ExciseMovement);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("AdministrativeReferenceCode", "20DE41000000001870745", dataProvider.ExciseMovement.AdministrativeReferenceCode);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence", "1", dataProvider.ExciseMovement.SequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED840D()
			{
				Header = new ED840DHeader()
				{
					MessageGroup = ED840DHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED840DBody()
				{
					EventReport = new ED840DBodyEventReport()
					{
						ExciseMovement = new ED840DBodyEventReportExciseMovement()
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						}
					}
				}
			};
			dataProvider = new ED840Provider(message);
		}
		ED840D message;
		IED840 dataProvider;

		protected override ED840Provider GetProvider() => (ED840Provider)dataProvider;
	}
}
