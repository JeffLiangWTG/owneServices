using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module
{
	public partial class EnquiryFilterControl : ZFilterStripControl<UPEZFilterStrip>
	{
		public EnquiryFilterControl(IBusinessObjectCollection gridCollection, UPEAirCargoFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
