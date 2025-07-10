using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
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

		public void TestDeclarationType()
		{
			AssertEquals("IM", provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", provider.AdditionalDeclarationType);
		}

		public void TestDeclarationAcceptanceDate()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.DeclarationAcceptanceDate);
		}

		public void TestReleaseDate()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.ReleaseDate);
		}

		public void TestResponseDateLimit()
		{
			AssertEquals(new DateTime(2023, 08, 15, 14, 30, 45), provider.ResponseDateLimit);
		}
		public void TestPreferredPaymentMethod()
		{
			AssertEquals("J", provider.PreferredPaymentMethod);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestGoodsItems()
		{
			AssertType<IM429GoodsItemProvider[]>(provider.GoodsItems);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM429Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM429.Im429
			{
				ImportOperation = new MCciOperationType46
				{
					Lrn = "LRN001",
					Mrn = "12MRN345ABCDE678R9",
					DeclarationType = "IM",
					AdditionalDeclarationType = "A",
					DeclarationAcceptanceDate = new DateTime(2023, 08, 10, 14, 30, 45),
					ReleaseDate = new DateTime(2023, 08, 11, 14, 30, 45),
					ResponseDateLimit = new DateTime(2023, 08, 15, 14, 30, 45),
					PreferredPaymentMethod = "J",
					Remarks = "Remarks001",
				},
				GoodsShipment = new Collection<MGoodsShipmentType01>()
				{
					new MGoodsShipmentType01()
					{
						SequenceNumber = "1",
						GoodsShipmentItem = new Collection<MGoodsShipmentItemType01>
						{
							new MGoodsShipmentItemType01()
						}
					}
				},
			});
		}
		IM429Provider provider;
	}
}
