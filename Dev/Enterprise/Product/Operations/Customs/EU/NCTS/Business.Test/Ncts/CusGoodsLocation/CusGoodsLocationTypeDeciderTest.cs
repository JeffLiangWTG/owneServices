using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusGoodsLocationTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		public void TestEUTypeDeciderIsSetupCorrectly_ParentIsEnRouteIncident()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				var goodsLocation = nctsHeader.EnRouteIncidents.AddNew().GoodsLocation;
				Factory.Save();

				var businessObject = new BusinessObjectFactory().Load(EUType, goodsLocation.PK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.DE.INctsCusGoodsLocation>(), businessObject.GetType());
			}
		}

		protected override Type BaseTypeDecidedType => typeof(CusGoodsLocation);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusGoodsLocation>();

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var movementHeader = (NctsArrivalMovementHeader)((CusGoodsLocation)bizO).Parent;
			movementHeader.Header.Branch.Company.GC_RN_NKCountryCode = countryCode;
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
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_GB = branch.PK;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.ArrivalMovementHeader.GoodsLocation;
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			var countryTypes = new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.INctsCusGoodsLocation>() },
			};
			return countryTypes;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			var countryTypes = new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsCusGoodsLocation>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.INctsCusGoodsLocation>() },
			};
			return countryTypes;
		}
	}
}
