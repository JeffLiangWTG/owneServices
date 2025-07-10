using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGoodsLocationAddressLookups Lookups => (CusGoodsLocationAddressLookups)base.Lookups;

		protected override JobDocAddressLookups GetNewLookups()
		{
			return new CusGoodsLocationAddressLookups(this);
		}

		public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation()
		{
			return new CusGoodsLocationAddressValidation(this);
		}
	}
}
