using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM428ProviderTest : TestCaseWithFactory
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
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestDeclarationAcceptanceDateAndTime()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.DeclarationAcceptanceDateAndTime);
		}

		public void TestDeclarationType()
		{
			AssertEquals("IM", provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", provider.AdditionalDeclarationType);
		}

		public void TestResponseDateLimit()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.ResponseDateLimit);
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
			provider = new IM428Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM428.Im428
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType45
				{
					Lrn = "LRN001",
					CustomsRegistrationNumber = "12CRN345ABCDE678R9",
					Mrn = "12MRN345CDEFG678R9",
					DeclarationAcceptanceDateAndTime = new DateTime(2023, 08, 10, 14, 30, 45),
					DeclarationType = "IM",
					AdditionalDeclarationType = "A",
					ResponseDateLimit = new DateTime(2023, 08, 11, 14, 30, 45),
					PreferredPaymentMethod = "A",
					Remarks = "Remarks001",
				}
			});
		}
		IM428Provider provider;
	}
}
