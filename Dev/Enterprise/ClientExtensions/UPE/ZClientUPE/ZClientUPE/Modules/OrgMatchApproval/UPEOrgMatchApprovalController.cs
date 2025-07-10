
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module
{
	public class UPEOrgMatchApprovalController : OrgMatchApprovalController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new UPEOrgMatchApprovalForm((OrgMatchApproval)businessEntity);
		}
	}
}
