using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CIncidentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("IncidentType missing", () => new CC043CIncidentProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals((ZShort)1, provider.SequenceNumber);
		}

		public void TestCode()
		{
			AssertEquals("001", provider.Code);
		}

		public void TestText()
		{
			AssertEquals("Incident 1 text", provider.Text);
		}

		public void TestEndorsement()
		{
			AssertType<CC043CEndorsementProvider>(provider.Endorsement);
		}

		public void TestEndorsement_Null()
		{
			var endorsementNullProvider = new CC043CIncidentProvider(new IncidentType04());
			AssertNull(endorsementNullProvider.Endorsement);
		}

		public void TestLocation()
		{
			AssertType<CC043CLocationProvider>(provider.Location);
		}

		public void TestLocation_Null()
		{
			var locationNullProvider = new CC043CIncidentProvider(new IncidentType04());
			AssertNull(locationNullProvider.Location);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CIncidentProvider(new IncidentType04
			{
				SequenceNumber = "1",
				Code = "001",
				Text = "Incident 1 text",
				Endorsement = new EndorsementType03()
				{
					Authority = "End Auth",
					Date = new DateTime(2023, 08, 22),
					Place = "Meath",
					Country = "IE",
				},
				Location = new LocationType02
				{
					QualifierOfIdentification = "QOI",
					UnLocode = "IEROS",
					Country = "IE"
				}
			});
		}
		CC043CIncidentProvider provider;
	}
}
