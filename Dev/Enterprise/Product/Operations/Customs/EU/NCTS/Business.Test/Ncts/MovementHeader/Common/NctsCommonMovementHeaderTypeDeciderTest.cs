using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsCommonMovementHeaderTypeDeciderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestNctsArrivalMovementHeader()
		{
			var header = CreateNctsHeader();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = header.ArrivalMovementHeader;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertType<NctsArrivalMovementHeader>("Base", new BusinessObjectFactory().Load<CusInBondMoveHeader>(arrivalMovementHeader.PK));
				AssertType<NctsArrivalMovementHeader>("Ncts Arrival", new BusinessObjectFactory().Load<NctsArrivalMovementHeader>(arrivalMovementHeader.PK));
				AssertType<NctsArrivalMovementHeader>("Ncts Base", new BusinessObjectFactory().Load<NctsCommonMovementHeader>(arrivalMovementHeader.PK));
			});
		}

		public void TestNctsDepartureMovementHeader()
		{
			var header = CreateNctsHeader();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovementHeader = header.MovementHeader;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertType<NctsDepartureMovementHeader>("Base", new BusinessObjectFactory().Load<CusInBondMoveHeader>(departureMovementHeader.PK));
				AssertType<NctsDepartureMovementHeader>("Ncts Departure", new BusinessObjectFactory().Load<NctsDepartureMovementHeader>(departureMovementHeader.PK));
				AssertType<NctsDepartureMovementHeader>("Ncts Base", new BusinessObjectFactory().Load<NctsCommonMovementHeader>(departureMovementHeader.PK));
			});
		}

		public void TestNctsUnloadingMovementHeader()
		{
			var header = CreateNctsHeader();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var unloadingMovementHeader = header.UnloadingMovementHeader;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertType<NctsUnloadingMovementHeader>("Base", new BusinessObjectFactory().Load<CusInBondMoveHeader>(unloadingMovementHeader.PK));
				AssertType<NctsUnloadingMovementHeader>("Ncts Unloading", new BusinessObjectFactory().Load<NctsUnloadingMovementHeader>(unloadingMovementHeader.PK));
				AssertType<NctsUnloadingMovementHeader>("Ncts Base", new BusinessObjectFactory().Load<NctsCommonMovementHeader>(unloadingMovementHeader.PK));
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
