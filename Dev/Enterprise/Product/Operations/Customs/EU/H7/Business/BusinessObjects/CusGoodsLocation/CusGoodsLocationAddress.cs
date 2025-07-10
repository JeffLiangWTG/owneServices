using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		#region Clone

		protected override bool SupportsCloneCore() => true;

		#endregion
	}
}
