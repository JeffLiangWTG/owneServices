using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderCargoLineCollection : DependentBusinessObjectCollection<CusSeaManOBLHeaderCargoLine, CusSeaManArrivalPort>
	{
		public CusSeaManOBLHeaderCargoLineCollection(CusSeaManArrivalPort port)
			: base(port)
		{
			this.port = port;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			CusSeaManOBLHeaderCargoLine line = (CusSeaManOBLHeaderCargoLine)child;
			line.BO_BT = port.BA_BT;
			line.BO_RL_NKDischargePort = port.BA_RL_NKArrivalPort;
		}

		#region Implementation

		readonly CusSeaManArrivalPort port;

		#endregion
	}
}
