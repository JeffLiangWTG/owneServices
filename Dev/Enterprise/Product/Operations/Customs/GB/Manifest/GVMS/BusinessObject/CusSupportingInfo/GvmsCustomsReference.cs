using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsCustomsReference : GvmsItemReference
	{
		public GvmsCustomsReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new GvmsCustomsReferenceLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new GvmsCustomsReferenceValidation(this);
		}
	}
}
