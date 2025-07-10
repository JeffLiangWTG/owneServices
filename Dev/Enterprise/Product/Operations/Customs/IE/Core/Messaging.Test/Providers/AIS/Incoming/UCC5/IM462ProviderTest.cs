using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM462;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	sealed class IM462ProviderTest : TestCase
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestCaseId()
		{
			AssertEquals("CID123", provider.CaseId);
		}

		public void TestAmendReason()
		{
			AssertEquals("Amend Reason", provider.AmendReason);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM462Provider(new Im462
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "CID123",
					AmendReason = "Amend Reason",
				},
			});
		}
		IM462Provider provider;
	}
}
