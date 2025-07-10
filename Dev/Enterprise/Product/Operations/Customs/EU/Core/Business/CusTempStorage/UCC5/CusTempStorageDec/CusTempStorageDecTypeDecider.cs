using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageDecTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type type = null;
			var countryCode = GetTypeForLoadCountryCode(row, factory);

			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Germany:
					var deTypeDeciderDE = (TypeDecider)ObjectFactory.Get<Integration.Customs.DE.ICusTempStorageDecTypeDecider>();
					type = deTypeDeciderDE.GetTypeForLoad(row, factory);
					break;
				case Core.Constants.CountryCodes.France:
					var deTypeDeciderFR = (TypeDecider)ObjectFactory.Get<Integration.Customs.FR.ICusTempStorageDecTypeDecider>();
					type = deTypeDeciderFR.GetTypeForLoad(row, factory);
					break;
				default:
					type = GetTypeForCountryCode(countryCode);
					break;
			}

			return type;
		}

		static ZString GetTypeForLoadCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var storageJobHeaderPK = (row != null) ? new ZGuid(row[CusTempStorageDec.Schema.STH_SJH]) : ZGuid.Invalid;
			var storageJobHeader = (storageJobHeaderPK.IsValid) ? factory.Load<CusTempStorageJobHeader>(storageJobHeaderPK) : null;
			return storageJobHeader?.CountryCode ?? Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusTempStorageDec);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusTempStorageDec>),
			};
	}
}
