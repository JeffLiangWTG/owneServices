using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM404ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("12AB345CDEFGH678R9", provider.CustomsRegistrationNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestAmendmentDateAndTime()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.AmendmentDateAndTime);
		}

		public void TestAmendmentAcceptanceDateAndTime()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.AmendmentAcceptanceDateAndTime);
		}

		public void TestPreferredPaymentMethod()
		{
			AssertEquals("A", provider.PreferredPaymentMethod);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestGoodsItems()
		{
			AssertType<IM404GoodsItemProvider[]>(provider.GoodsItems);
		}

		protected override void SetUp()
		{
			base.SetUp();

			provider = new IM404Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM404.Im404
			{
				ImportOperation = new MCciOperationType47
				{
					Lrn = "LRN001",
					CustomsRegistrationNumber = "12AB345CDEFGH678R9",
					Mrn = "12MRN345CDEFG678R9",
					AmendmentDateAndTime = new DateTime(2023, 08, 10, 14, 30, 45),
					AmendmentAcceptanceDateAndTime = new DateTime(2023, 08, 11, 14, 30, 45),
					PreferredPaymentMethod = "A",
					Remarks = "Remarks001",
				},
				GoodsShipment = new GoodsShipmentIm404Type()
				{
					GoodsShipmentItem = new Collection<GoodsShipmentItemIm404Type>()
					{
						new GoodsShipmentItemIm404Type()
					}
				},
			});
		}
		IM404Provider provider;
	}
}
