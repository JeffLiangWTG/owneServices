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

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodeTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (row != null)
			{
				var parentID = new ZGuid(row[NctsEuOfficeCode.Schema.CY_ParentID]);
				var parentTableCode = new ZString(row[NctsEuOfficeCode.Schema.CY_ParentTableCode]).ToUpperInvariant();

				if (parentID.IsValid)
				{
					if (parentTableCode == CusInBondHeaderSchema.Constants.Prefix)
					{
						var nctsHeader = factory.Load<NctsHeader>(parentID);
						countryCode = nctsHeader?.CountryCode ?? countryCode;
					}
					else if (parentTableCode == CusInBondMoveHeaderSchema.Constants.Prefix)
					{
						var nctsMovementHeader = factory.Load<NctsCommonMovementHeader>(parentID);
						countryCode = nctsMovementHeader?.CountryCode ?? countryCode;
					}
				}
			}
			return GetTypeForCountryCode(countryCode);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context) => GetTypeForLoad(null, null);

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsEuOfficeCode);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.INctsEuOfficeCode>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.INctsEuOfficeCode>),
			new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.INctsEuOfficeCode>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsEuOfficeCode>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsEuOfficeCode>),
		};
	}
}
