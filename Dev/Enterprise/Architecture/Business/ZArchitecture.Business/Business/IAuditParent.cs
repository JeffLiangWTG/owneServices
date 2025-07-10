using System.Collections.Generic;
using Enterprise.ZArchitecture.Business.Business;

namespace Enterprise.ZArchitecture.Business
{
	public interface IAuditParent
	{
		IEnumerable<AuditChildInfo> RelatedAuditChildren { get; }
	}
}
