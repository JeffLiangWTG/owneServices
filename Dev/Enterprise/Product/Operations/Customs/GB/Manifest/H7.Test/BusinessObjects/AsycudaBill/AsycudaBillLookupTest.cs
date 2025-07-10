using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList_Contents()
		{
			SetUpHeaderBill();
			var expectedMessageStatusList = new Common.Shared.MessageStatusList();

			AssertContainsExactElementsInAnyOrder(expectedMessageStatusList.GetAllCodes(), bill.Lookups.MessageStatusList.GetAllCodes());
		}

		public void TestCustomsStatusList_Contents()
		{
			SetUpHeaderBill();
			var expectedEntryStatusList = new List<string> { "ACC", "RCV", "CTL", "DOC", "TAX", "CLR", "CAN" };

			AssertContainsExactElementsInAnyOrder(expectedEntryStatusList, bill.Lookups.CustomsStatusList.GetAllCodes());
		}

		public void TestSubStyleList()
		{
			SetUpHeaderBill();
			SetUpRegionName();

			CombineAssertions(() =>
			{
				header.AMA_RL_NKPortOfDischarge = "";
				var expectedDefaultSubStyleListCodes = new List<string> { "A", "D", "J", "K" };
				AssertContainsExactElementsInAnyOrder(expectedDefaultSubStyleListCodes, bill.Lookups.SubStyleList.GetAllCodes());

				header.AMA_RL_NKPortOfDischarge = "GBLON";
				var expectedBIRDSSubStyleListCodes = new List<string> { "J", "K" };
				AssertContainsExactElementsInAnyOrder(expectedBIRDSSubStyleListCodes, bill.Lookups.SubStyleList.GetAllCodes());

				header.AMA_RL_NKPortOfDischarge = "GBBEL";
				var expectedSubStyleListCodes = new List<string> { "A", "D" };
				AssertContainsExactElementsInAnyOrder(expectedSubStyleListCodes, bill.Lookups.SubStyleList.GetAllCodes());
			});
		}

		public void TestAdditionalProcedureSubStyleListDifferent()
		{
			SetUpHeaderBill();
			var additionalProcedureList = bill.Lookups.AdditionalProcedureList;
			var subStyleList = bill.Lookups.SubStyleList;

			AssertNotEquals(additionalProcedureList, subStyleList);
		}

		void SetUpHeaderBill()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}

		void SetUpRegionName()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "XXX";
				refCountryStates.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = refCountryStates.PK;
			}
			var london = new RefUNLOCO.Loader(Factory).Load("GBLON");
			if (london.CountryStates == null || string.Compare(london.CountryStates.RW_RegionName, "ENGLAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "YYY";
				refCountryStates.RW_RegionName = "ENGLAND";
				london.RL_RW = refCountryStates.PK;
			}
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
