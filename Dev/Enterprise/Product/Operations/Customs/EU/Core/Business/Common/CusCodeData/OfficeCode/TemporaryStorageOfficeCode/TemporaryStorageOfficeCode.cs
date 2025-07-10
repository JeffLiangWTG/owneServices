using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.EU.Business
{
	public class TemporaryStorageOfficeCode : EuOfficeCode
	{
		public TemporaryStorageOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(TemporaryStorageHeader));

		protected override CusCodeDataValidation GetNewValidation() => new TemporaryStorageOfficeCodeValidation(this);

		public new TemporaryStorageOfficeCodeValidation Validation => (TemporaryStorageOfficeCodeValidation)base.Validation;
	}
}
