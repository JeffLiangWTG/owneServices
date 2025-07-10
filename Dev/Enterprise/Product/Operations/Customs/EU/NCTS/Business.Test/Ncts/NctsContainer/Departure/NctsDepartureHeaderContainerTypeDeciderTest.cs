using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsDepartureHeaderContainerTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		public void TestGetTypeForNewWithITypeDeciderContext()
		{
			var frCompany = Factory.New<GlbCompany>();
			frCompany.GC_Code = "CFR";
			frCompany.GC_Name = "FR Company";
			frCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var frBranch = frCompany.Branches.AddNew();
			frBranch.GB_Code = "BFR";

			var lvHeader = Factory.New<NctsHeader>();
			var frHeader = Factory.New<NctsHeader>();
			frHeader.BH_GB = frBranch.PK;

			var typeDecider = new NctsDepartureHeaderContainerTypeDecider();
			AssertEquals("FR", ObjectFactory.GetType<Integration.Customs.FR.IFRNctsDepartureHeaderContainer>(), typeDecider.GetTypeForNew(frHeader));
			AssertEquals("LV", typeof(NctsDepartureHeaderContainer), typeDecider.GetTypeForNew(lvHeader));
		}

		protected override Type BaseTypeDecidedType => typeof(NctsDepartureHeaderContainer);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.NCTS.INctsDepartureHeaderContainer>();

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
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
			var header = Factory.New<NctsHeader>();
			header.BH_GB = branch.PK;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.DepartureHeaderContainers.AddNew();
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			((NctsDepartureHeaderContainer)bizO).Header.Branch.Company.GC_RN_NKCountryCode = countryCode;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests() => new Dictionary<string, Type>
		{
			{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsDepartureHeaderContainer>() },
			{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IFRNctsDepartureHeaderContainer>() },
			{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsDepartureHeaderContainer>() },
		};

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsDepartureHeaderContainer>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IFRNctsDepartureHeaderContainer>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsDepartureHeaderContainer>() },
			};
		}
	}
}
