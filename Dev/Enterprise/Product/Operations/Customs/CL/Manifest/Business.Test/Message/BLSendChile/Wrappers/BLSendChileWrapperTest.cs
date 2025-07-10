using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLSendChileWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		readonly string actionType = WrappersConstants.ActionType.A;

		[TestDate(2021, 03, 10, 12, 00, 00)]
		public void TestBLSendChileWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);

			CreateAndPopulateHouseBill(true);

			AsycudaBill bill = header.Bills[0];
			CreateAndPopulatePack(bill);

			IBLRequest wrapper = new BLSendChileWrapper(bill, actionType);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.ActionType.A, wrapper.ActionType);
				AssertEquals("(H)QRHW20050055C", wrapper.ReferenceNumber);
				AssertEquals("01-02-2021 12:00", wrapper.Date);
				AssertEquals(BLSendChileConstants.ServiceDocument.Liner, wrapper.Service);
				AssertEquals(BLSendChileConstants.ServiceTypeDocument.RoRo, wrapper.ServiceType);
				AssertEquals((ZString)"1", wrapper.ItemsAmount);
				AssertEquals((ZString)"170.000", wrapper.Weight);
				AssertEquals("Kgm", wrapper.WeightUQ);
				AssertEquals((ZString)"0.76", wrapper.Volume);
				AssertEquals("Mtq", wrapper.VolumeUQ);
				AssertEquals((ZString)"1", wrapper.ItemsTotal);
			});
		}

		public void TestServiceType()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();
			CreateAndPopulateHouseBill(false);

			IBLRequest wrapper = new BLSendChileWrapper(header.Bills[0], actionType);
			Factory.Save();

			AssertEquals(BLSendChileConstants.ServiceTypeDocument.Bb, wrapper.ServiceType);

			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals(BLSendChileConstants.ServiceTypeDocument.Bb, wrapper.ServiceType);

			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.Containers[0].ACN_EmptyFullIndicator = "FCL";
			AssertEquals(BLSendChileConstants.ServiceTypeDocument.FclFcl, wrapper.ServiceType);

			header.Containers[0].ACN_EmptyFullIndicator = "LCL";
			AssertEquals(BLSendChileConstants.ServiceTypeDocument.LclLcl, wrapper.ServiceType);
		}

		void PopulateManifestHeader()
		{
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CNT-001";
			container.ACN_EmptyFullIndicator = "FCL";
		}

		void CreateAndPopulateHouseBill(ZBool roro)
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 170;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = roro;
			bill.ABL_Volume = 0.76;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
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
