using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CommonPreviousDocument : EU.NCTS.Business.CommonPreviousDocument
	{
		public CommonPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected new CommonPreviousDocumentLookups Lookups => (CommonPreviousDocumentLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new CommonPreviousDocumentLookups(this);
	}
}
