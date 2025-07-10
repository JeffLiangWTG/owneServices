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
	public class CusTempStorageJobHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var branchPK = (row != null) ? new ZGuid(row[CusTempStorageJobHeader.Schema.SJH_GB]) : ZGuid.Invalid;
			var decBranch = (branchPK.IsValid) ? factory.Load<GlbBranch>(branchPK) : null;
			var storageJobHeaderCountryCode = decBranch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return GetTypeForCountryCode(storageJobHeaderCountryCode);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new (Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusTempStorageJobHeader>),
				new (Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageJobHeader>),
				new (Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusTempStorageJobHeader>),
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusTempStorageJobHeader);
	}
}
