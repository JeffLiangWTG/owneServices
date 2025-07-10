using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotUnderbondMovementRequestHeader : CusSCAOceanBillUnderbondMovementRequestHeader
	{
		public CusSCAPivotUnderbondMovementRequestHeader(CusUnderbond underbond, CusSCAPivot pivot)
			: base(underbond, pivot.OceanBill)
		{
			this.pivot = pivot;
		}

		public override ZString TranshipmentOverseasDestinationPort
		{
			get
			{
				return underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.Transshipment ?
					underbond.C4_RL_NKTranshipDestPort : ZString.Empty;
			}
		}

		public override IUnderbondMovementRequestLine Line
		{
			get { return new CusSCAPivotUnderbondMovementRequestLine(pivot); }
		}

		#region Implementantion

		readonly CusSCAPivot pivot;

		#endregion
	}
}
