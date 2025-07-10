using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class OpCharacteristicsWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestOpCharacteristicsWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			PopulateManifestHeader();
			CreateAndPopulateContainer(EmptyFullIndicatorList.Codes.LessThanFullContainerLoad, "22G0");
			CreateAndPopulateHouseBill();
			CreateAndPopulateMasterBill();

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var opCharacteristics = wrapper.Master.OpCharacteristics;

			CombineAssertions(() =>
			{
				AssertEquals(ZBool.False, opCharacteristics.Conditions);
				AssertEquals(ZBool.False, opCharacteristics.CarrierResponsability);
				AssertEquals(COWrappersConstants.LCLSlashLCL, opCharacteristics.NegotiationType);
				AssertEquals("3", opCharacteristics.LoadType);
				AssertEquals(ZBool.True, opCharacteristics.Precursors);
				AssertEquals(0m, opCharacteristics.USDFOBValue);
				AssertEquals(0m, opCharacteristics.USDFreightValue);
				AssertEquals(ZString.Empty, opCharacteristics.Marks);
				AssertEquals(1, opCharacteristics.ContainerQty);
				AssertEquals(10, opCharacteristics.PackageQty);
				AssertEquals(0.1m, opCharacteristics.TotalGrossWeight);
				AssertEquals(0.05m, opCharacteristics.TotalVolume);
				AssertEquals("UY", opCharacteristics.LoadingCountryCode);
				AssertEquals("UYMVD", opCharacteristics.LoadingPlaceCode);
				AssertEquals("2", opCharacteristics.DeliveryMode);
			});

			opCharacteristics = wrapper.Master.Houses.ElementAt(0).OpCharacteristics;

			CombineAssertions(() =>
			{
				AssertEquals(ZBool.True, opCharacteristics.Conditions);
				AssertEquals(ZBool.True, opCharacteristics.CarrierResponsability);
				AssertEquals(COWrappersConstants.LCLSlashLCL, opCharacteristics.NegotiationType);
				AssertEquals("5", opCharacteristics.LoadType);
				AssertEquals(ZBool.True, opCharacteristics.Precursors);
				AssertEquals(120.23m, opCharacteristics.USDFOBValue);
				AssertEquals(130.23m, opCharacteristics.USDFreightValue);
				AssertEquals("MARKSANDNUMBERS", opCharacteristics.Marks);
				AssertEquals(1, opCharacteristics.ContainerQty);
				AssertEquals(10, opCharacteristics.PackageQty);
				AssertEquals(0.1m, opCharacteristics.TotalGrossWeight);
				AssertEquals(0.05m, opCharacteristics.TotalVolume);
				AssertEquals("CL", opCharacteristics.LoadingCountryCode);
				AssertEquals("CLSCL", opCharacteristics.LoadingPlaceCode);
				AssertEquals("2", opCharacteristics.DeliveryMode);
			});
		}

		public void TestNegotiationTypeFullContainerLoad()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			PopulateManifestHeader();
			CreateAndPopulateContainer(EmptyFullIndicatorList.Codes.FullContainerLoad, "22G0");
			CreateAndPopulateHouseBill();
			CreateAndPopulateMasterBill();

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var opCharacteristics = wrapper.Master.OpCharacteristics;

			AssertEquals(COWrappersConstants.FCLSlashFCL, opCharacteristics.NegotiationType);
		}

		public void TestNegotiationTypeCombined()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			PopulateManifestHeader();
			CreateAndPopulateContainer(EmptyFullIndicatorList.Codes.FullContainerLoad, "22G0");
			CreateAndPopulateContainer(EmptyFullIndicatorList.Codes.LessThanFullContainerLoad, "22G1");
			CreateAndPopulateHouseBill();
			CreateAndPopulateMasterBill();

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var opCharacteristics = wrapper.Master.OpCharacteristics;

			AssertEquals(COWrappersConstants.LCLSlashLCL, opCharacteristics.NegotiationType);
		}

		void PopulateManifestHeader()
		{
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_ManifestNumber = "MAN0100";
			header.Precursors = ZBool.True;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
			header.DeliveryMode = CODeliveryModeList.Codes._2;
		}

		void CreateAndPopulateHouseBill()
		{
			var bill = header.Bills.AddNew();

			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_GrossWeight = 100;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			bill.ABL_ManifestQty = 10;
			bill.ABL_Volume = 50;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			bill.ABL_FreightValue = 130.231m;
			bill.ABL_GoodsValue = 120.231m;
			bill.ABL_MarksAndNumbers = "MARKSANDNUMBERS";
			bill.ABL_RL_NKOrigin = "CLSCL";
			bill.CarriersLiability = ZBool.True;
			bill.Multimodal = ZBool.True;
			bill.ABL_ContainerMode = "5";

			var pack = bill.Packs.AddNew();
			pack.ContainerPK = header.Containers[0].PK;
		}

		void CreateAndPopulateMasterBill()
		{
			var bill = header.MasterBill;

			bill.ABL_BillNumber = "MEDUQ2394375";
			bill.ABL_RL_NKPortOfLoading = "UYMVD";
			bill.CarriersLiability = ZBool.False;
			bill.Multimodal = ZBool.False;
		}

		void CreateAndPopulateContainer(ZString emptyFullIndicator, ZString code)
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = code;

			var container = header.Containers.AddNew();
			container.ACN_RC_ContainerType = refContainer.PK;
			container.ACN_EmptyFullIndicator = emptyFullIndicator;
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
