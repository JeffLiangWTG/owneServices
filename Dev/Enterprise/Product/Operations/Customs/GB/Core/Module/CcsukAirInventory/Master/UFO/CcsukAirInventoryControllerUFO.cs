using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukAirInventoryControllerUFO : CcsukAirInventoryController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventoryUFO; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CcsukAirInventoryUFOForm(mawb);
		}

		internal void SetNewBusinessObjectToReturn(CusMAWB mawb)
		{
			this.mawb = mawb;
		}
		CusMAWB mawb;
	}
}
