using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderTypeDecider : CountrySpecificTypeDecider, Integration.Customs.EU.ITemporaryStorageHeaderTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ITemporaryStorageHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ITemporaryStorageHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ITemporaryStorageHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ITemporaryStorageHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ITemporaryStorageHeader>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(TemporaryStorageHeader);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var branchPK = (row != null) ? new ZGuid(row[AsycudaManifestHeader.Schema.AMA_GB]) : ZGuid.Invalid;
			var decBranch = (branchPK.IsValid) ? factory.Load<GlbBranch>(branchPK) : null;
			var storageHeaderCountryCode = decBranch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			storageHeaderCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(storageHeaderCountryCode);
			return GetTypeForCountryCode(storageHeaderCountryCode);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(TemporaryStorageHeader);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			var country = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return GetTypeForCountryCode(country);
		}
	}
}
