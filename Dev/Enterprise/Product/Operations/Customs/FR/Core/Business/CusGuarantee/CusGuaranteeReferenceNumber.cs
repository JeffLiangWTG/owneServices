using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business
{
	public class CusGuaranteeReferenceNumber : Customs.Business.CusGuaranteeReferenceNumber
	{
		public CusGuaranteeReferenceNumber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGuaranteeHeader CusGuarantee => (CusGuaranteeHeader)base.Parent;

		public new CusGuaranteeReferenceNumberValidation Validation => (CusGuaranteeReferenceNumberValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CusGuaranteeReferenceNumberValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusGuaranteeHeader));
	}
}
