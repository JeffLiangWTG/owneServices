using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageBillTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ITemporaryStorageBill>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ITemporaryStorageBill>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ITemporaryStorageBill>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ITemporaryStorageBill>)
		};

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return this.GetCorrectEUTypeForCountryCode(GetBillCountryCode(row, factory), DefaultTypeForUnsupportedCountry);
		}

		public override Type GetTypeForBinding()
		{
			return this.GetCorrectEUTypeForCountryCode(CurrentCountryCode, DefaultTypeForUnsupportedCountry);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			return this.GetCorrectEUTypeForCountryCode(CurrentCountryCode, DefaultTypeForUnsupportedCountry);
		}

		protected override Type DefaultTypeForEuCountry => typeof(TemporaryStorageBill);
		protected override Type DefaultTypeForUnsupportedCountry => typeof(TemporaryStorageBill);

		protected ZString GetBillCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = (row != null) ? new ZGuid(row[AsycudaBillSchema.Constants.ABL_AMA]) : ZGuid.Invalid;
			var header = (headerPK.IsValid) ? factory.Load<TemporaryStorageHeader>(headerPK) : null;
			var storageHeaderCountryCode = header?.Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(storageHeaderCountryCode);
		}
	}
}
