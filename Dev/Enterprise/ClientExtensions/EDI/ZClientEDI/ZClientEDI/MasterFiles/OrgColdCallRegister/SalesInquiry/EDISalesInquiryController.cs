using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDISalesInquiryController : SalesEnquiryController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDISalesInquiryForm((EDISalesInquiry)businessEntity);
		}
	}
}
