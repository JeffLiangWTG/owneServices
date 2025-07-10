using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusSCAOceanBillUnderbondMovementRequestHeader : CusUnderbondUnderbondMovementRequestHeader
	{
		public CusSCAOceanBillUnderbondMovementRequestHeader(CusUnderbond underbond, CusSCAOceanBill oceanBill)
			: base(underbond)
		{
			this.oceanBill = oceanBill;
		}

		readonly CusSCAOceanBill oceanBill;

		public override ZString VesselID
		{
			get
			{
				return oceanBill.CB_LloydsIMO;
			}
		}

		public override bool IsBureau
		{
			get { return oceanBill.CB_IsBureau; }
		}

		public override ZString VoyageNumber
		{
			get { return oceanBill.CB_Voyage; }
		}
	}
}
