using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgOpportunity : DocumentWrapper
	{
		DocOrgOpportunity(OrgOpportunity orgOpportunity, BusinessObjectFactory factoryToWrap)
			: base(orgOpportunity, factoryToWrap)
		{
		}

		public static DocOrgOpportunity New(OrgOpportunity orgOpportunity, BusinessObjectFactory factoryToWrap)
		{
			return (orgOpportunity != null) ? new DocOrgOpportunity(orgOpportunity, factoryToWrap) : null;
		}

		public OrgOpportunity OrgOpportunity
		{
			get { return (OrgOpportunity)WrappedObject; }
		}
	}
}
