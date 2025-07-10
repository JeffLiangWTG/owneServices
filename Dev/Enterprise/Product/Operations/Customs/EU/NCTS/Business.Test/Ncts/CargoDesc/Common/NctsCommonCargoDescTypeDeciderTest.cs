using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsCommonCargoDescTypeDeciderTest : TestCaseWithFactory
	{
		public void TestNctsArrivalAndUnloadingCargoDesc_Arrival()
		{
			var header = CreateNctsHeader();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItem = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertType<NctsArrivalCargoDesc>("Base", new BusinessObjectFactory().Load<CusInBondCargoDesc>(goodsItem.PK));
				AssertType<NctsArrivalCargoDesc>("Ncts Arrival", new BusinessObjectFactory().Load<NctsArrivalCargoDesc>(goodsItem.PK));
				AssertType<NctsArrivalCargoDesc>("Ncts Base", new BusinessObjectFactory().Load<NctsCommonCargoDesc>(goodsItem.PK));
			});
		}

		public void TestNctsDepartureCargoDesc()
		{
			var header = CreateNctsHeader();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertType<NctsDepartureCargoDesc>("Base", new BusinessObjectFactory().Load<CusInBondCargoDesc>(goodsItem.PK));
				AssertType<NctsDepartureCargoDesc>("Ncts Departure", new BusinessObjectFactory().Load<NctsDepartureCargoDesc>(goodsItem.PK));
				AssertType<NctsDepartureCargoDesc>("Ncts Base", new BusinessObjectFactory().Load<NctsCommonCargoDesc>(goodsItem.PK));
			});
		}

		public void TestNctsArrivalAndUnloadingCargoDesc_Unloading()
		{
			var header = CreateNctsHeader();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItem = header.UnloadingMovementHeader.GoodsItems.AddNew();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertType<NctsArrivalAndUnloadingCargoDesc>("Base", new BusinessObjectFactory().Load<CusInBondCargoDesc>(goodsItem.PK));
				AssertType<NctsArrivalAndUnloadingCargoDesc>("Ncts Unloading", new BusinessObjectFactory().Load<NctsArrivalAndUnloadingCargoDesc>(goodsItem.PK));
				AssertType<NctsArrivalAndUnloadingCargoDesc>("Ncts Base", new BusinessObjectFactory().Load<NctsCommonCargoDesc>(goodsItem.PK));
			});
		}

		NctsHeader CreateNctsHeader()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C#@";
			company.GC_Name = "COMP TEST";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B#@";
			branch.GB_BranchName = "BRANCH TEST";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_GB = branch.PK;
			return header;
		}
	}
}
