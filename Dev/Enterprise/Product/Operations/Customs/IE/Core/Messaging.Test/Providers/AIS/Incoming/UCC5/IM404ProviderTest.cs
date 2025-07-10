using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM404;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	class IM404ProviderTest : TestCaseWithFactory
	{
		public void TestAmendmentAcceptanceDate()
		{
			AssertEquals(new ZDate(2024, 03, 01), provider.AmendmentAcceptanceDate);
		}

		public void TestItems()
		{
			AssertEquals(1, provider.GoodsShipment.Items.Count);
		}

		protected override void SetUp()
		{
			provider = new IM404Provider(new Im404()
			{
				Declaration = new DeclarationType()
				{
					AmendmentAcceptanceDate = "20240301",
					Mrn = "MRN12345",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IEDUB100"
					}
				},
				GoodsShipment = new GoodsShipmentType()
				{
					GoodsShipmentItem = new System.Collections.ObjectModel.Collection<GoodsShipmentTypeItem>()
					{
						new GoodsShipmentTypeItem()
					}
				}
			});
		}
		IM404Provider provider;
	}
}
