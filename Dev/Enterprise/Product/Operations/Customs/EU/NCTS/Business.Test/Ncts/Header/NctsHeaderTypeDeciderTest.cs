using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class NctsHeaderTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var header = bizO as NctsHeader;
			if (header != null)
			{
				header.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

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
			var header = Factory.NewWithValidTestData<NctsHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			header.BH_GB = branch.PK;
			return header;
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			var countryTypes = new Dictionary<ZGuid, Type>
			{
				{ Enterprise.Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusInBondHeader>() },
			};
			return countryTypes;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			var countryTypes = new Dictionary<string, Type>
			{
				{ Enterprise.Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusInBondHeader>() },
				{ Enterprise.Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusInBondHeader>() },
			};
			return countryTypes;
		}

		public void TestAllCountriesIncludingLatviaAreTested()
		{
			var typeDecider = new NctsHeaderTypeDecider();
			CombineAssertions(() =>
			{
				AssertEquals("GetTestCountryPKsAndExpectedTypesForLoad", typeDecider.CountrySpecificTypes.Count() + 1, GetTestCountryPKsAndExpectedTypesForLoad().Count);
				AssertEquals("GetTestCountryCodesForNewAndBindingTests", typeDecider.CountrySpecificTypes.Count() + 1, GetTestCountryCodesForNewAndBindingTests().Count);
			});
		}

		protected override Type BaseTypeDecidedType => typeof(NctsHeader);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader>();
	}
}
