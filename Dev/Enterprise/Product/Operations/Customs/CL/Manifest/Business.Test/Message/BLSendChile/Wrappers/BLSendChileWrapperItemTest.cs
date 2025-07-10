using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLSendChileWrapperItemTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLSendChileWrapperItem()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateHouseBill();

			AsycudaBill bill = header.Bills[0];
			CreateAndPopulatePack(bill);

			IBLRequest wrapper = new BLSendChileWrapper(bill, WrappersConstants.ActionType.A);
			IDocumentItem item = wrapper.Items.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("1", item.Number);
				AssertEquals("S/I", item.GoodMarks);
				AssertEquals(false, item.DangerousGoods);
				AssertEquals("64", item.BulkType);
				AssertEquals("1X20´DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400", item.GoodDescription);
				AssertEquals("1", item.PackageQty);
				AssertEquals("170.000", item.GrossWeight);
				AssertEquals("Kgm", item.WeightUQ);
				AssertEquals("0.76", item.Volume);
				AssertEquals("Mtq", item.VolumeUQ);
				AssertEquals(false, item.Containerized);
			});
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 170;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = true;
			bill.ABL_Volume = 0.76;
			bill.ABL_VolumeUQ = "M3";
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "1X20´DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400";
			pack.APA_LineNo = 1;
			pack.APA_MarksAndNumbers = "S/I";
			pack.APA_PackQty = 1;
			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			pack.APA_Volume = 0.76;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicMetres;
			pack.APA_Weight = 170;
			pack.APA_WeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}
