using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM429ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", provider.AdditionalDeclarationType);
		}

		public void TestPreferredPaymentMethod()
		{
			AssertEquals("J", provider.PreferredPaymentMethod);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestGetEntryStatus()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals("REL", provider.GetEntryStatus(message));
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM429Provider(new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM429.Im429
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM429.DeclarationType
				{
					Lrn = "LRN001",
					Mrn = "12MRN345ABCDE678R9",
					AdditionalDeclarationType = "A",
					PreferredPaymentMethod = "J",
					Remarks = "Remarks001",
				},
			});
		}

		IM429Provider provider;
	}
}

