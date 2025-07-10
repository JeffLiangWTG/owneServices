using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class IcsOfficeCode : EuOfficeCode
	{
		public IcsOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusCodeDataLookups GetNewLookups() => new IcsOfficeCodeLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => (Parent as AsycudaManifestHeader)?.GetIcsOfficeCodeValidation(this) ?? new IcsOfficeCodeValidation(this);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(ObjectFactory.GetType(typeof(EUManifest.IAsycudaManifestHeader)));
	}
}
