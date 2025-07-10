using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEUEntryHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetEUEntryHeaderCountryCode(row, factory));

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.ICusEUEntryHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusEUEntryHeader>(); }),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusEUEntryHeader);

		protected ZString GetEUEntryHeaderCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var entryHeaderPK = (row != null) ? new ZGuid(row[CusEUEntryHeader.Schema.EUH_CH]) : ZGuid.Invalid;
			var entryHeader = (entryHeaderPK.IsValid) ? (CusEntryHeader)factory.Load(typeof(CusEntryHeader), entryHeaderPK) : null;

			var countryCode = entryHeader?.CountryCode ?? ZString.Empty;
			return countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
		}
	}
}
