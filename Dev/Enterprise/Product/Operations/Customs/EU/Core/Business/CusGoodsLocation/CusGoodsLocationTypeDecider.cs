using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Country = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Customs.EU.Business
{
	public class CusGoodsLocationTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (row == null)
			{
				return GetTypeForCountryCode(ZString.Empty);
			}

			var parentTableCode = row[CusGoodsLocationSchema.Constants.CGL_ParentTableCode].ToString().ToUpperInvariant();
			var parentID = new ZGuid(row[CusGoodsLocationSchema.Constants.CGL_ParentID]);
			var countryCode = ZString.Empty;
			switch (parentTableCode)
			{
				case CusEntryInstructionSchema.Constants.Prefix:
					countryCode = factory.Load<CusEntryInstruction>(parentID)?.CountryCode ?? ZString.Empty;
					break;
				case JobDeclarationSchema.Constants.Prefix:
					countryCode = factory.Load<JobDeclaration>(parentID)?.CountryCode ?? ZString.Empty;
					break;
				case AsycudaManifestHeaderSchema.Constants.Prefix:
					return CusTempStorage.CusGoodsLocation.TypeDecider.GetTypeForCountryCode(factory.Load<TemporaryStorageHeader>(parentID)?.AMA_RN_NKCountry ?? ZString.Empty);
				case AsycudaBillSchema.Constants.Prefix:
					countryCode = factory.Load<AsycudaBill>(parentID)?.Header.AMA_RN_NKCountry ?? ZString.Empty;
					break;
			}
			return GetTypeForCountryCode(countryCode);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new (Country.Italy, ObjectFactory.GetType<IT.ICusGoodsLocation>),
			new (Country.Spain, ObjectFactory.GetType<ES.ICusGoodsLocation>),
			new (Country.Germany, ObjectFactory.GetType<DE.ICusGoodsLocation>),
			new (Country.Belgium, ObjectFactory.GetType<BE.ICusGoodsLocation>),
			new (Country.Netherlands, ObjectFactory.GetType<NL.ICusGoodsLocation>),
			new (Country.France, ObjectFactory.GetType<FR.ICusGoodsLocation>),
			new (Country.Ireland, ObjectFactory.GetType<IE.ICusGoodsLocation>),
			new (Country.Poland, ObjectFactory.GetType<PL.ICusGoodsLocation>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusGoodsLocation);
	}
}
