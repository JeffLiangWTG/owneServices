using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business
{
	public interface IWebDocumentsSupportBase
	{
		ZGuid DocParentPK { get; }
		List<ZGuid> DocRelatedPKs { get; }
	}
}
