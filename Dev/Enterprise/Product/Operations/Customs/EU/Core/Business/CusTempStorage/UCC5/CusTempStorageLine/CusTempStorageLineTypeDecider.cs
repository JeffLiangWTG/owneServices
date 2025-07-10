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
	public class CusTempStorageLineTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var storageDecPK = row != null ? new ZGuid(row[CusTempStorageLine.Schema.TSL_STH]) : ZGuid.Invalid;
			var storageDec = storageDecPK.IsValid ? factory.Load<CusTempStorageDec>(storageDecPK) : null;
			var countryCode = storageDec?.StorageHeader?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			if (countryCode == Core.Constants.CountryCodes.Germany)
			{
				var deTypeDecider = (TypeDecider)ObjectFactory.Get<Integration.Customs.DE.ICusTempStorageLineTypeDecider>();
				return deTypeDecider.GetTypeForLoad(row, factory);
			}

			return GetTypeForCountryCode(countryCode);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageLine>),
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusTempStorageLine);

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageLine>();
	}
}
