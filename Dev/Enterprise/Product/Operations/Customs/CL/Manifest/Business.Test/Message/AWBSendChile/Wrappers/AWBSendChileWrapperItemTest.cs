using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperItemTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestAWBSendChileWrapperItem()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateHouseBill();
			CreateAndPopulateArrivalInfo();
			CreateAndPopulatePack();

			IAWBRequest wrapper = new AWBSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Gral);
			IDocItems item = wrapper.DocItems.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert(wrapper.DocItems.IsCountEqualTo(1));

				AssertEquals("1", item.Number);
				AssertEquals(false, item.DangerousGoods);
				AssertEquals("20", item.PackageQty);
				AssertEquals("100.000", item.GrossWeight);
				AssertEquals("Kgm", item.WeightUQ);
				AssertEquals("0.76", item.Volume);
				AssertEquals("Mtq", item.VolumeUQ);
			});
		}

		ZGuid CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_GrossWeight = 170;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_ManifestQty = 1;
			bill.ABL_Volume = 0.76;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			return bill.PK;
		}

		void CreateAndPopulateArrivalInfo()
		{
			AsycudaArrivalHeader arrivalHeader = header.ArrivalHeaders.AddNew();
			AsycudaArrivalLine arrivalLine = (AsycudaArrivalLine)arrivalHeader.ArrivalDetails.AddNew();
			arrivalLine.ATL_ABL_AsycudaBill = header.Bills[0].PK;
			arrivalLine.ATL_Quantity = 20;
			arrivalLine.ATL_Weight = 100;
			arrivalLine.ATL_WeightUQ = Core.Constants.Weight.Kilograms;
		}

		void CreateAndPopulatePack()
		{
			AsycudaPack pack = header.Bills[0].Packs.AddNew();
			pack.APA_LineNo = 1;
			pack.APA_PackQty = 1;
			pack.APA_Volume = 0.76;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicMetres;
			pack.APA_Weight = 170;
			pack.APA_WeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}
