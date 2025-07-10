using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM410ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("12CRN345ABCDE678R9", provider.CustomsRegistrationNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestInvalidationDecisionDateAndTime()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.InvalidationDecisionDateAndTime);
		}

		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.InvalidationRequestDateAndTime);
		}

		public void TestInvalidationInitiatedByCustoms()
		{
			AssertEquals("0", provider.InvalidationInitiatedByCustoms);
		}

		public void TestInvalidationJustification()
		{
			AssertEquals("Invalidation Justification", provider.InvalidationJustification);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM410Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM410.Im410
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType23
				{
					Lrn = "LRN001",
					CustomsRegistrationNumber = "12CRN345ABCDE678R9",
					Mrn = "12MRN345ABCDE678R9",
					InvalidationDecisionDateAndTime = new DateTime(2023, 08, 10, 14, 30, 45),
					InvalidationRequestDateAndTime = new DateTime(2023, 08, 11, 14, 30, 45),
					InvalidationInitiatedByCustoms = "0",
					InvalidationJustification = "Invalidation Justification",
				}
			});
		}
		IM410Provider provider;
	}
}
