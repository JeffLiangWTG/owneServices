using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestAWBSendChileWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var billPK = CreateAndPopulateHouseBill();
			CreateAndPopulateArrivalInfo(billPK);

			IAWBRequest wrapper = new AWBSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Mot);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.ActionType.A, wrapper.ActionType);
				AssertEquals("(H)QRHW20050055C", wrapper.ReferenceNumber);
				AssertEquals((ZString)"1", wrapper.ItemsAmount);
				AssertEquals((ZString)"127.000", wrapper.Weight);
				AssertEquals("Kgm", wrapper.WeightUQ);
				AssertEquals((ZString)"1000.00", wrapper.Volume);
				AssertEquals("Mtq", wrapper.VolumeUQ);
				AssertEquals((ZString)"1", wrapper.ItemsTotal);
				AssertEquals(true, wrapper.IsPartial);
				AssertEquals("1", wrapper.PartialCorrelative);
			});
		}

		ZGuid CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 127.000m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";

			bill.Packs.AddNew();

			return bill.PK;
		}

		void CreateAndPopulateArrivalInfo(ZGuid billPk)
		{
			AsycudaArrivalHeader arrivalHeader = header.ArrivalHeaders.AddNew();
			AsycudaArrivalLine arrivalLine = (AsycudaArrivalLine)arrivalHeader.ArrivalDetails.AddNew();
			arrivalLine.ATL_ABL_AsycudaBill = billPk;
			arrivalLine.ATL_Quantity = 20;
		}
	}
}
