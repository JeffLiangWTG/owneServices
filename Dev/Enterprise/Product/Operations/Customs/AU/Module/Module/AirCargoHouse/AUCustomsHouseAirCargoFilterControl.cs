using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public partial class AUCustomsHouseAirCargoFilterControl : ZFilterStripControl<WorkflowFilterStrip>
	{
		public AUCustomsHouseAirCargoFilterControl()
		{
			InitializeComponent();
		}

		public AUCustomsHouseAirCargoFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.Value)
			{
				grid.RemoveFromAvailableColumns(CusHAWB.Schema.CS_fPartShipConsignmentReference);
			}
			if (!Registry.Business.HVLVDataRegistry.HasHVLVClearance)
			{
				grid.RemoveFromAvailableColumns(CusHAWB.Schema.InBondStore);
			}
		}
	}
}
