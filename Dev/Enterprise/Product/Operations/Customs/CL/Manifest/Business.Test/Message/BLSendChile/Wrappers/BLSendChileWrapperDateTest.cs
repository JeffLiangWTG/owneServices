using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLSendChileWrapperDateTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		[TestDate(2021, 03, 10, 12, 00, 00)]
		public void TestBLSendChileWrapperDate()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();
			CreateAndPopulateHouseBill(true);

			IBLRequest wrapper = new BLSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A);

			IDocumentDate date1 = wrapper.Dates.ElementAt(0);
			IDocumentDate date2 = wrapper.Dates.ElementAt(1);
			IDocumentDate date3 = wrapper.Dates.ElementAt(2);
			IDocumentDate date4 = wrapper.Dates.ElementAt(3);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.DateName.Fpres, date1.Name);
				AssertEquals("10-03-2021 12:00", date1.Value);
				AssertEquals(WrappersConstants.DateName.Fem, date2.Name);
				AssertEquals("01-02-2021", date2.Value);
				AssertEquals(WrappersConstants.DateName.Fzarpe, date3.Name);
				AssertEquals("03-02-2021 12:00", date3.Value);
				AssertEquals(WrappersConstants.DateName.Femb, date4.Name);
				AssertEquals("03-02-2021 12:00", date4.Value);
			});
		}

		void PopulateManifestHeader()
		{
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_E_DEP = new ZDate(2021, 02, 03);
			header.AMA_MasterBillIssueDate = new ZDate(2021, 02, 01);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
		}

		void CreateAndPopulateHouseBill(bool roro)
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 127.000m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = roro;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";
		}
	}
}
