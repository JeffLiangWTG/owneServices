using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.UPE.Module
{
	public partial class UPEAirCargoCalloutBaseFilterControl : ZFilterStripControl<UPEZFilterStrip>
	{
		public UPEAirCargoCalloutBaseFilterControl(IBusinessObjectCollection gridCollection, UPEAirCargoCalloutBaseFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
