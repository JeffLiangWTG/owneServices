using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public partial class JobRequiredDocumentAddInfoFilterStripControl : MasterFiles.Module.JobRequiredDocumentAddInfoFilterStripControl
	{
		public JobRequiredDocumentAddInfoFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			using (base.grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				base.grid.SetColumnCaption(JobRequiredDocumentAddInfo.Schema.EX_ReferenceNumber, Res.GetString("88DAF541-A6CD-49A5-A5FE-B8BFF73A240F", "URN"));
			}
		}
	}
}
