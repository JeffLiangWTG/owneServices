using System.Linq;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class BLArgentinaWrapperPackTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLArgentinaWrapperPack()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			CreateAndPopulateContainer();
			CreateAndPopulatePack(bill);
			CreateAndPopulatePack(bill, false);

			Factory.Save();

			ISeaManifest wrapper = new BLArgentinaWrapperManifest(bill);
			IPack pack1 = wrapper.Packs.ElementAt(0);
			IPack pack2 = wrapper.Packs.ElementAt(1);

			CombineAssertions(() =>
			{
				AssertEquals(1, pack1.LineNumber);
				AssertEquals("9", pack1.PackUQ);
				AssertEquals(1, pack1.PackQuantity);
				AssertEquals(170.111m, pack1.VolumeWeight);
				AssertEquals("1X20´DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400", pack1.Description);
				AssertEquals("S/I", pack1.MarksAndNumbers);
				AssertEquals("P", pack1.ContainerCondition);

				AssertEquals(2, pack2.LineNumber);
				AssertEquals(0.762m, pack2.VolumeWeight);
			});
		}

		void CreateAndPopulatePack(AsycudaBill bill, bool isWeightFilled = true)
		{
			var pack = bill.Packs.AddNew();

			pack.APA_GoodsDescription = "1X20´DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400";
			pack.APA_MarksAndNumbers = "S/I";
			pack.APA_PackQty = 1;
			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			if (isWeightFilled)
			{
				pack.APA_LineNo = 1;
				pack.APA_Weight = 170.111;
			}
			else
			{
				pack.APA_LineNo = 2;
				pack.APA_Volume = 0.762;
			}
			pack.ContainerPK = header.Containers[0].PK;
		}

		void CreateAndPopulateContainer()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "22G0";

			var container = header.Containers.AddNew();
			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
			container.ACN_RC_ContainerType = refContainer.PK;
		}
	}
}
