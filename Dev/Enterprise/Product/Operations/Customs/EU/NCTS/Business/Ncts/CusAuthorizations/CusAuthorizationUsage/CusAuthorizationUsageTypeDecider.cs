using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusAuthorizationUsageTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsCusAuthorizationUsage>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusAuthorizationUsage);

		protected override Type DefaultTypeForEuCountry => typeof(CusAuthorizationUsage);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCountryCode(row, factory));

		protected ZString GetCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			string countryCode = null;
			if (row != null)
			{
				var parentID = new ZGuid(row[CusAuthorizationUsage.Schema.AGC_ParentID]);
				var parentTableCode = new ZString(row[CusAuthorizationUsage.Schema.AGC_ParentTableCode]).ToUpperInvariant();

				if (parentID.IsValid)
				{
					if (parentTableCode == CusInBondHeaderSchema.Constants.Prefix)
					{
						var nctsHeader = factory.Load<NctsHeader>(parentID);
						countryCode = nctsHeader?.CountryCode;
					}
					else if (parentTableCode == CusInBondMoveHeaderSchema.Constants.Prefix)
					{
						var nctsMovementHeader = factory.Load<NctsCommonMovementHeader>(parentID);
						countryCode = nctsMovementHeader?.Header?.CountryCode;
					}
				}
			}

			return countryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
