using CargoWise.EntityFramework;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.UPE.Module
{
	public partial class UPEAirCargoFilterControl : UPEAirCargoCalloutBaseFilterControl
	{
		public UPEAirCargoFilterControl(IBusinessObjectCollection gridCollection, UPEAirCargoFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
