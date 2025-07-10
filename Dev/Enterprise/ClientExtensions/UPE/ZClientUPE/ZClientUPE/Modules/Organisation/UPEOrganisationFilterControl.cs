using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Module
{
	public partial class UPEOrganisationFilterControl : OrganisationFilterControl
	{
		public UPEOrganisationFilterControl()
		{
			InitializeComponent();
		}

		public UPEOrganisationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
