using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	public sealed class CusGoodsLocationTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EUH7.ICusGoodsLocation>();

		protected override Type BaseTypeDecidedType => typeof(CusGoodsLocation);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.CusGoodsLocation;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			var countryTypes = new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.ITH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IEH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ESH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.GBH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FRH7.ICusGoodsLocation>() },
			};
			return countryTypes;
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var bill = (bizO as CusGoodsLocation).Parent;
			var header = bill.Header;

			header.SuspendCheckBusinessObjectType();
			header.AMA_RN_NKCountry = countryCode;
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			var countryTypes = new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.ITH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.GBH7.ICusGoodsLocation>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FRH7.ICusGoodsLocation>() },
			};
			return countryTypes;
		}
	}
}
