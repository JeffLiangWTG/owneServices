using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.EndpointManagement.Module
{
	public partial class EdiTrustedMessagingConfigFilterControl : ZFilterStripControl
	{
		public EdiTrustedMessagingConfigFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}