using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsUnloadingMovementHeaderTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			var countryTypes = new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IUnloadingMovementHeader>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.IUnloadingMovementHeader>() }
			};
			return countryTypes;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var departureMovement = bizO as NctsUnloadingMovementHeader;
			if (departureMovement != null)
			{
				departureMovement.Header.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B#@";
			branch.GB_BranchName = "BRANCH TEST";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.UnloadingMovementHeader;
		}

		protected override Type BaseTypeDecidedType => typeof(NctsUnloadingMovementHeader);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.NCTS.IUnloadingMovementHeader>();

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IUnloadingMovementHeader>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.IUnloadingMovementHeader>() }
			};
		}
	}
}
