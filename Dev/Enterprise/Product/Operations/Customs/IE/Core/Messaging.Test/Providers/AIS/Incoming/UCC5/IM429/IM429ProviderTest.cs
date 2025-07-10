using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing.UCC5
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

		public void TestResponseDateLimit()
		{
			AssertEquals("15-Aug-23", provider.ResponseDateLimit.ToShortDateString());
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
			AssertType<Messaging.UCC5.IM429GoodsItemProvider[]>(provider.GoodsItems);
		}

		public void TestAcceptanceDate()
		{
			AssertEquals("15-Apr-24", provider.AcceptanceDate.ToShortDateString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new Messaging.UCC5.IM429Provider(new Im429
			{
				Declaration = new DeclarationType()
				{
					Lrn25 = "LRN001",
					Mrn = "12MRN345ABCDE678R9",
					DeclarationType11 = "IM",
					AdditionalDeclarationType12 = "A",
					ResponseDateLimit = "20230815",
					PreferredPaymentMethod48 = "J",
					Remarks = "Remarks001",
				},
				GoodsShipment = new GoodsShipmentType()
				{
					DatesPlaces = new GoodsShipmentTypeDatesPlaces() { AcceptanceDate531 = "20240415" },
					GoodsShipmentItem = new Collection<GoodsShipmentItemType>
					{
						new GoodsShipmentItemType()
						{
							GoodsItemNumber16 = "1",
							Taxes = new TaxesType()
						}
					}
				},
			});
		}
		Messaging.UCC5.IM429Provider provider;
	}
}

