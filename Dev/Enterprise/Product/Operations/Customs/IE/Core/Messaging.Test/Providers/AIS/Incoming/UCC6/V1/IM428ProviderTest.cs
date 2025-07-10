using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM428ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestDeclarationAcceptanceDateAndTime()
		{
			AssertEquals(new DateTime(2023, 08, 10), provider.DeclarationAcceptanceDateAndTime);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("IM", provider.AdditionalDeclarationType);
		}

		public void TestPreferredPaymentMethod()
		{
			AssertEquals("A", provider.PreferredPaymentMethod);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM428Provider(new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM428.Im428
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM428.DeclarationType
				{
					Lrn = "LRN001",
					Mrn = "12MRN345CDEFG678R9",
					AcceptanceDate = "20230810",
					AdditionalDeclarationType = "IM",
					PreferredPaymentMethod = "A",
					Remarks = "Remarks001",
				},
			});
		}
		IM428Provider provider;
	}
}

