using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DeviceManagement.Module
{
	public partial class ClientDeviceHeaderTemplateFilterControl : ZFilterStripControl
	{
		public ClientDeviceHeaderTemplateFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
