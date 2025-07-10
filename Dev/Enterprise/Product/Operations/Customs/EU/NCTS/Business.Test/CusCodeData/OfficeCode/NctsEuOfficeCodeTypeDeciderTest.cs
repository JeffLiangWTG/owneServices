using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	internal class NctsEuOfficeCodeTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(NctsEuOfficeCode);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.NCTS.INctsEuOfficeCode>();

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
			return header.MovementHeader.CustomsOffices.AddNew();
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			((NctsDepartureMovementHeader)((NctsEuOfficeCode)bizO).Parent).Header.Branch.Company.GC_RN_NKCountryCode = countryCode;
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
			{ Enterprise.Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.INctsEuOfficeCode>() },
			{ Enterprise.Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.INctsEuOfficeCode>() },
			{ Enterprise.Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.INctsEuOfficeCode>() },
			{ Enterprise.Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsEuOfficeCode>() },
			{ Enterprise.Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsEuOfficeCode>() },
		};

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Enterprise.Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.INctsEuOfficeCode>() },
				{ Enterprise.Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.INctsEuOfficeCode>() },
				{ Enterprise.Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.INctsEuOfficeCode>() },
				{ Enterprise.Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsEuOfficeCode>() },
				{ Enterprise.Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsEuOfficeCode>() },
			};
		}
	}
}
