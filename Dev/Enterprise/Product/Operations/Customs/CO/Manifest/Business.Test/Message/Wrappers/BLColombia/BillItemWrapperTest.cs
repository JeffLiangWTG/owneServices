using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class BillItemWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestItemWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;

			var container = CreateAndPopulateContainer();
			var bill = CreateAndPopulateHouseBill();

			CreateAndPopulatePack(bill, container.PK);
			CreateAndPopulatePack(bill, container.PK);

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var item = wrapper.Master.Houses.ElementAt(0).Item;

			CombineAssertions(() =>
			{
				AssertEquals("4", item.BulkType);
				AssertEquals(ZString.Empty, item.ContainerIDNumber);
				AssertEquals(ZString.Empty, item.Size);
				AssertEquals(ZString.Empty, item.EquipmentType);
				AssertEquals(ZString.Empty, item.SealNumber);
				AssertEquals(0.3m, item.GrossWeight);
				AssertEquals(1, item.BulkQty);
				AssertEquals(0.12m, item.Volume);

				AssertEquals(2, item.Packs.Count);
			});
		}

		AsycudaBill CreateAndPopulateHouseBill()
		{
			var bill = header.Bills.AddNew();

			bill.ABL_GrossWeight = 300m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			bill.ABL_ManifestQty = 1;
			bill.ABL_Volume = 123.233m;
			bill.ABL_ContainerMode = "4";
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicDecimetres;

			return bill;
		}

		void CreateAndPopulatePack(AsycudaBill bill, ZGuid containerPK)
		{
			var pack = bill.Packs.AddNew();

			pack.APA_Volume = 100;
			pack.ContainerPK = containerPK;
		}

		AsycudaContainer CreateAndPopulateContainer()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "22G0";
			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.FlatRack;

			var containerMap = Factory.New<RefContainerCodeMap>();
			containerMap.RCM_RC_Container = refContainer.PK;
			containerMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Colombia;
			containerMap.RCM_Code = "1";

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CAIU305178-6";
			container.ACN_Seal1 = "SEAL";
			container.ACN_GoodsWeight = 200m;
			container.ACN_NumberOfPackages = 2;
			container.ACN_RC_ContainerType = refContainer.PK;

			return container;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932049";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;
		}
	}
}
