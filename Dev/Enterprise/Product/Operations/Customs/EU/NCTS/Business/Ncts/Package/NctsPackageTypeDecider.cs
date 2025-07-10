using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsPackageTypeDecider : CountrySpecificTypeDecider
		, Integration.Customs.EU.NCTS.INctsPackageTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.INctsPackage>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsPackage>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.INctsPackage>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsPackage>)
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsPackage);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var goodsItemPk = row == null ? ZGuid.Empty : new ZGuid(row[NctsPackage.Schema.B5_ParentID]);
			var goodsItem = goodsItemPk.IsValid ? factory.Load<NctsCommonCargoDesc>(goodsItemPk) : null;
			var countryCode = goodsItem?.Header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return GetTypeForCountryCode(countryCode);
		}
	}
}
