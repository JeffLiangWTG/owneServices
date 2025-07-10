using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class ContainerItemWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestItemWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			header.TravelDocumentType = COWrappersConstants.ContainerNumberNeeded;

			var container = CreateAndPopulateContainer();
			var bill = CreateAndPopulateHouseBill();

			CreateAndPopulatePack(bill, container.PK);
			CreateAndPopulatePack(bill, container.PK);

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));

			var item = wrapper.Master.Items.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals("3", item.BulkType);
				AssertEquals("CAIU305178-6", item.ContainerIDNumber);
				AssertEquals("1", item.Size);
				AssertEquals("2", item.EquipmentType);
				AssertEquals("SEAL", item.SealNumber);
				AssertEquals(44.4m, item.GrossWeight);
				AssertEquals(2, item.BulkQty);
				AssertEquals(0.22m, item.Volume);
			});
		}

		public void TestItemWrapperContainerVolumeAndGrossWeight()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;

			var container = header.Containers.AddNew();
			var bill = CreateAndPopulateHouseBill();

			CreateAndPopulatePack(bill, container.PK);
			CreateAndPopulatePack(bill, container.PK);

			var container2 = header.Containers.AddNew();
			var bill2 = CreateAndPopulateHouseBill();

			CreateAndPopulatePack(bill2, container2.PK);
			CreateAndPopulatePack(bill2, container2.PK);
			CreateAndPopulatePack(bill2, container2.PK);

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));

			var item = wrapper.Master.Items.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals(0.22m, item.Volume);
				AssertEquals(44.4m, item.GrossWeight);
				AssertEquals(2, item.Packs.Count);
			});

			item = wrapper.Master.Items.ElementAt(1);

			CombineAssertions(() =>
			{
				AssertEquals(0.33m, item.Volume);
				AssertEquals(66.6m, item.GrossWeight);
				AssertEquals(3, item.Packs.Count);
			});
		}

		AsycudaBill CreateAndPopulateHouseBill()
		{
			var bill = header.Bills.AddNew();

			bill.ABL_ManifestQty = 1;
			bill.ABL_Volume = 123.233m;

			return bill;
		}

		void CreateAndPopulatePack(AsycudaBill bill, ZGuid containerPK)
		{
			var pack = bill.Packs.AddNew();

			pack.APA_Volume = 111.111m;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			pack.APA_Weight = 222;
			pack.APA_WeightUQ = Core.Constants.Weight.Hectograms;
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
