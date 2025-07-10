using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsArrivalMovementHeaderTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			var countryTypes = new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IArrivalMovementHeader>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IArrivalMovementHeader>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IArrivalMovementHeader>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.IArrivalMovementHeader>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.IArrivalMovementHeader>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IArrivalMovementHeader>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IArrivalMovementHeader>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.IArrivalMovementHeader>() },
			};
			return countryTypes;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var arrivalMovement = bizO as NctsArrivalMovementHeader;
			if (arrivalMovement != null)
			{
				arrivalMovement.Header.Branch.Company.GC_RN_NKCountryCode = countryCode;
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
			return nctsHeader.ArrivalMovementHeader;
		}

		protected override Type BaseTypeDecidedType => typeof(NctsArrivalMovementHeader);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.NCTS.IArrivalMovementHeader>();

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IArrivalMovementHeader>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IArrivalMovementHeader>() },
			};
		}
	}
}
