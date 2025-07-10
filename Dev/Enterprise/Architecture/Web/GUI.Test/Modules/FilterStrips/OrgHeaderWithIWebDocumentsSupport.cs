using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class OrgHeaderWithIWebDocumentsSupport : OrgHeader, IWebDocumentsSupportBase
	{
		public OrgHeaderWithIWebDocumentsSupport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid DocParentPK => PK;

		public List<ZGuid> DocRelatedPKs => DocRelatedPKs_Exposed;

		public List<ZGuid> DocRelatedPKs_Exposed;
	}
}
