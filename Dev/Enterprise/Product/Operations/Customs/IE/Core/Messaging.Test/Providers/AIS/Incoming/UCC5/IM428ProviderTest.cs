using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	sealed class IM428ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN25", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN123456", provider.MovementReferenceNumber);
		}

		public void TestDeclarationAcceptanceDate()
		{
			AssertEquals("20250201", provider.DeclarationAcceptanceDate);
		}

		public void TestGoodsItems()
		{
			AssertType(typeof(IM428GoodsItemProviderUCC5[]), provider.GoodsItems);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM428Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.Im428
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.DeclarationType()
				{
					Lrn25 = "LRN25",
					Mrn = "MRN123456",
					AcceptanceDate = "20250201"
				},
				GoodsShipment = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.GoodsShipmentType()
				{
					GoodsShipmentItem = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.GoodsShipmentTypeItem>
					{
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.GoodsShipmentTypeItem
						{
							GoodsItemNumber16 = "1",
						}
					}
				}
			});
		}
		IM428Provider provider;
	}
}
