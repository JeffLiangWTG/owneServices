using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperProdItemTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		AsycudaBill bill;

		public void TestAWBSendChileWrapperProdItem()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			CreateAndPopulatePack();

			IAWBRequest wrapper = new AWBSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Gral);
			IDocItems item = wrapper.DocItems.ElementAt(0);
			IDocProdItems iproditem = item.DocProdItems.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert(item.DocProdItems.IsCountEqualTo(1));

				AssertEquals("20", iproditem.Quantity);
				AssertEquals("Description", iproditem.Description);
				AssertEquals("Gli", iproditem.MeasureUQ);
			});
		}

		void CreateAndPopulatePack()
		{
			AsycudaPack pack = bill.Packs.AddNew();
			pack.APA_PackQty = 20;
			pack.APA_PackUQ = "GA";
			pack.APA_GoodsDescription = "Description";
		}
	}
}
