using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderTypeDecider : CountrySpecificTypeDecider, Integration.Customs.EU.NCTS.ICusInBondHeaderTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var branchPK = row != null ? new ZGuid(row[CusInBondHeader.Schema.BH_GB]) : ZGuid.Invalid;
			var branch = branchPK.IsValid ? factory.Load<GlbBranch>(branchPK) : null;
			var countryCode = branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			return GetTypeForCountryCode(countryCode);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusInBondHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusInBondHeader>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsHeader);
	}
}
