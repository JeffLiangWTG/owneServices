using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	internal sealed class WebOrgMiscServ : LightweightBizo<OrgMiscServ>
	{
		public WebOrgMiscServ(OrgMiscServ miscServ)
			: base(miscServ)
		{
		}
	}
}
