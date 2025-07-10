using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaManifestHeaderTypeDecider : CountrySpecificTypeDecider, Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeaderTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEH7.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.GBH7.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESH7.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FRH7.IH7ManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.ITH7.IAsycudaManifestHeader>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(AsycudaManifestHeader);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var country = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return GetTypeForCountryCode(country);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaManifestHeader);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			var country = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return GetTypeForCountryCode(country);
		}
	}
}
