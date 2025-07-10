using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderCollection))]
	class NctsHeaderCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestGetCompanyQuery()
		{
			var gbBranch = SetupGBCompanyAndBranch();

			var gbHeader = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival);
			gbHeader.BH_GB = gbBranch.PK;
			var lvHeader = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival);
			Factory.Save();

			var collection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var collectionFilter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Different Company", false, gbHeader.MatchesFilter(collectionFilter));
				AssertEquals("Current Company", true, lvHeader.MatchesFilter(collectionFilter));
			});
		}

		public void TestGetCompanyQueryWithAdditionalQuery()
		{
			var gbBranch = SetupGBCompanyAndBranch();

			var departureHeader = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure);
			var arrivalHeader = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival);
			var gbArrivalHeader = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival);
			gbArrivalHeader.BH_GB = gbBranch.PK;
			Factory.Save();

			var collection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany, new ZQuery(CusInBondHeaderSchema.BH_HeaderType, NctsMovementType.Codes.Arrival));
			var collectionFilter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Departure", false, departureHeader.MatchesFilter(collectionFilter));
				AssertEquals("Current Company Arrival", true, arrivalHeader.MatchesFilter(collectionFilter));
				AssertEquals("Different Company Arrival", false, gbArrivalHeader.MatchesFilter(collectionFilter));
			});
		}

		public void TestGetApplicationFilter()
		{
			var gbBranch = SetupGBCompanyAndBranch();
			var inBondHeader = SetupHeader(CusInBondApplicationCodeList.Codes.InBond, NctsMovementType.Codes.Departure);
			var nc4Header = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure);
			var nc5Header = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival);
			var gbArrivalHeader = SetupHeader(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival);
			gbArrivalHeader.BH_GB = gbBranch.PK;
			Factory.Save();

			var collection = new NctsHeaderCollection(Factory);
			var collectionFilter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Not NCTS", false, inBondHeader.MatchesFilter(collectionFilter));
				AssertEquals("Phase 4", true, nc4Header.MatchesFilter(collectionFilter));
				AssertEquals("Phase 5", true, nc5Header.MatchesFilter(collectionFilter));
				AssertEquals("GB Phase 5", true, gbArrivalHeader.MatchesFilter(collectionFilter));
			});
		}

		public void TestFetchStrategyType()
		{
			AssertType<NctsHeaderCollectionFetchStrategy>(Collection.FetchStrategy);
		}

		protected override Type GetExpectedCollectionType() => typeof(NctsHeaderCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new NctsHeaderCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}

		GlbBranch SetupGBCompanyAndBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "GBDVR";
			branch.GB_Code = "OTH";
			return branch;
		}

		NctsHeader SetupHeader(ZString applicationCode, ZString movementType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = applicationCode;
			header.SetMovementType(movementType);
			return header;
		}
	}
}
