using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CusGoodsLocationAddress : EU.NCTS.Business.CusGoodsLocationAddress, IDocAddress
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

		protected override bool AuthorisationNumberReadOnly =>
			base.AuthorisationNumberReadOnly
			&& (GoodsLocation == null || GoodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.AuthorizationNumber);

		protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

		ZString IDocAddress.E2_Address1 =>
			GoodsLocation is CusGoodsLocation cusGoodsLocation && cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.PostcodeAddress
			? cusGoodsLocation.AdditionalIdentifier
			: E2_Address1;
	}
}
