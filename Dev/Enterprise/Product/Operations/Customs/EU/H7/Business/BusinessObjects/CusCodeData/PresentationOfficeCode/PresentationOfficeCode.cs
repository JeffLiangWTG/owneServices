using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class PresentationOfficeCode : EuOfficeCode
	{
		public PresentationOfficeCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusCodeDataLookups GetNewLookups() => new PresentationOfficeCodeLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => new PresentationOfficeCodeValidation(this);

		public new PresentationOfficeCodeLookups Lookups => (PresentationOfficeCodeLookups)base.Lookups;

		public new PresentationOfficeCodeValidation Validation => (PresentationOfficeCodeValidation)base.Validation;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaManifestHeader));
	}
}
