using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class GlbMauExternalPasswordLookups : GlbExternalPasswordLookups
{
	public GlbMauExternalPasswordLookups(GlbMauExternalPassword parent) : base(parent)
	{
	}

	public OrgHeaderCollection Declarants => new OrgHeaderCollection(Factory);
}
