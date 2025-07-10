using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	sealed class IM451ProviderTest : TestCase
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN123", provider.LocalReferenceNumber);
		}

		public void TestDeclarationType()
		{
			AssertEquals("CO", provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", provider.AdditionalDeclarationType);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Rejection Reason", provider.RejectionReason);
		}

		public void TestPreferredPaymentMethod()
		{
			AssertEquals("B", provider.PreferredPaymentMethod);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM451Provider(new Im451
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					Lrn25 = "LRN123",
					DeclarationType11 = "CO",
					AdditionalDeclarationType12 = "A",
					RejectionReason = "Rejection Reason",
					PreferredPaymentMethod48 = "B",
					Remarks = "Remarks001",
				},
			});
		}
		IM451Provider provider;
	}
}
