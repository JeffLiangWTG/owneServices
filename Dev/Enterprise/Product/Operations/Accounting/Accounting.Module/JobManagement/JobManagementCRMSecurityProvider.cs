using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	class JobManagementCRMSecurityProvider : CRMSecurityProvider<JobHeader>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => new SchemaColumn[] { JobHeaderSchema.JH_GS_NKRepSales };

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => new SchemaGuidColumn[] { JobHeaderSchema.JH_OA_LocalChargesAddr };

		public override CRMSecurity CRMSecurity => Env.Security.JobManagementCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;
	}
}
