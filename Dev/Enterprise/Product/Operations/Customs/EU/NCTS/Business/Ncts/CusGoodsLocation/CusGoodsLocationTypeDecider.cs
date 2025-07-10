using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusGoodsLocationTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			NctsHeader header = null;
			var parentTableCode = row[CusGoodsLocationSchema.Constants.CGL_ParentTableCode].ToString();
			var parentID = new ZGuid(row[CusGoodsLocationSchema.Constants.CGL_ParentID]);
			if (parentTableCode == CusInBondMoveHeaderSchema.Constants.Prefix)
			{
				header = factory.Load<NctsCommonMovementHeader>(parentID)?.Header;
			}
			else if (parentTableCode == CusInBondEventSchema.Constants.Prefix)
			{
				header = factory.Load<EnRouteIncident>(parentID)?.Header;
			}
			return GetTypeForCountryCode(header?.CountryCode ?? ZString.Empty);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsCusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.INctsCusGoodsLocation>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusGoodsLocation);
	}
}
