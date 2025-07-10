using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CusGoodsLocation : EU.H7.Business.CusGoodsLocation, Integration.Customs.ESH7.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

		public new CusGoodsLocationAddress Address => base.Address as CusGoodsLocationAddress;

		protected override Type AddressType => typeof(CusGoodsLocationAddress);

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		protected override TypeLoaderCollection GetParentLoaders() => new TypeLoaderCollection(typeof(AsycudaBill), typeof(AsycudaManifestHeader));
	}
}
