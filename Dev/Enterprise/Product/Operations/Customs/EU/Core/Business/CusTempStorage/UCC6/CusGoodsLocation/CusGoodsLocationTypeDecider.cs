using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Country = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusGoodsLocationTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			ICusGoodsLocationTypeSupporter header = null;
			var parentTableCode = row[CusGoodsLocationSchema.Constants.CGL_ParentTableCode].ToString();
			var parentID = new ZGuid(row[CusGoodsLocationSchema.Constants.CGL_ParentID]);
			if (parentTableCode == AsycudaManifestHeaderSchema.Constants.Prefix)
			{
				header = (TemporaryStorageHeader)factory.Load<AsycudaManifestHeader>(parentID);
			}

			return header == null ? GetTypeForCountryCode(ZString.Empty) : header.GoodsLocationType;
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new (Country.Ireland, ObjectFactory.GetType<IE.ICusGoodsLocation>),
			new (Country.Italy, ObjectFactory.GetType<IT.ITemporaryStorageCusGoodsLocation>),
			new (Country.Spain, ObjectFactory.GetType<ES.ICusGoodsLocation>),
			new (Country.Germany, ObjectFactory.GetType<DE.ICusGoodsLocation>),
			new (Country.Belgium, ObjectFactory.GetType<BE.ICusGoodsLocation>),
			new (Country.Netherlands, ObjectFactory.GetType<NL.ICusGoodsLocation>),
			new (Country.France, ObjectFactory.GetType<FR.ICusGoodsLocation>),
			new (Country.Poland, ObjectFactory.GetType<PL.ICusGoodsLocation>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusGoodsLocation);
	}
}
