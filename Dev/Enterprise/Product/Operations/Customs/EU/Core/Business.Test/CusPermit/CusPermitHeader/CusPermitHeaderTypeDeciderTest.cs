using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusPermitHeaderTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestTypeDecider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var permit = Factory.New<CusPermitHeader>();
				permit.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
				permit.CPH_StartDate = ZDate.BrettsBirthday;
				permit.CPH_Number = "12345";
				permit.CPH_Type = "OPL";
				permit.CPH_OH_PermitHolder = orgHeader.PK;

				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				AssertNoExceptionThrown(delegate
				{
					newFactory.Load<CusPermitHeader>(permit.PK);
					newFactory.Load<Integration.Customs.GB.ICusEntryHeader>(permit.PK);
				});
			}
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var permit = bizO as CusPermitHeader;
			if (permit != null)
			{
				permit.CPH_RN_NKCountryCode = countryCode;
			}
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusPermitHeader>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusPermitHeader>() },
			};
		}

		protected override Type BaseTypeDecidedType => typeof(CusPermitHeader);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
			permit.CPH_StartDate = ZDate.BrettsBirthday;
			permit.CPH_Number = "12345";
			permit.CPH_Type = "OPL";
			permit.CPH_OH_PermitHolder = orgHeader.PK;

			return permit;
		}
	}
}
