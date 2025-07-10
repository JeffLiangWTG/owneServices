using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CreditStatus;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class CreditStatusBizObjForDocumentApproval : CreditStatusBusinessObject
	{
		public CreditStatusBizObjForDocumentApproval(BusinessObjectFactory factory, IEnumerable<ZGuid> organisationPKsInBreach)
			: base(factory)
		{
			orgPKsInBreach = organisationPKsInBreach;
		}

		readonly IEnumerable<ZGuid> orgPKsInBreach;

		public override OrgHeaderCollection Headers
		{
			get
			{
				if (headers == null)
				{
					var query = new ZQuery(OrgHeaderSchema.PK, orgPKsInBreach);
					headers = new OrgHeaderCollection(Factory, query);
					headers.Load();
				}
				return headers;
			}
		}
		OrgHeaderCollection headers;
	}
}
