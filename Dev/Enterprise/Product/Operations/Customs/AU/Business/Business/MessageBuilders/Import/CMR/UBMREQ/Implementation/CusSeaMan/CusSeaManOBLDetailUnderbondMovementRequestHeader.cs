using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailUnderbondMovementRequestHeader : CusUnderbondUnderbondMovementRequestHeader
	{
		public CusSeaManOBLDetailUnderbondMovementRequestHeader(CusUnderbond underbond, CusSeaManOBLDetail detail)
			: base(underbond)
		{
			this.detail = detail;
		}

		public override ZString VesselID
		{
			get { return detail.Header.TransportHeader.BT_LloydsIMO; }
		}

		public override ZString VoyageNumber
		{
			get { return detail.Header.TransportHeader.BT_VoyageNum; }
		}

		public override bool IsBureau
		{
			get { return false; }
		}

		public override ZString TranshipmentOverseasDestinationPort
		{
			get
			{
				ZString result = ZString.Empty;

				if (underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.Transshipment
					&& detail.Header != null
					&& !detail.Header.BO_RL_NKDestinationPort.StartsWith(Core.Constants.CountryCodes.Australia))
				{
					result = detail.Header.BO_RL_NKDestinationPort;
				}

				return result;
			}
		}

		public override IUnderbondMovementRequestLine Line
		{
			get { return new CusSeaManOBLDetailUnderbondMovementRequestLine(detail); }
		}

		#region Implementation

		readonly CusSeaManOBLDetail detail;

		#endregion
	}
}
