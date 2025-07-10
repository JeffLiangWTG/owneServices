namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBUnderbondMovementRequestHeader : CusMAWBUnderbondMovementRequestHeader
	{
		public CTOCusHAWBUnderbondMovementRequestHeader(CusUnderbond underbond, CTOCusHAWB hAWB)
			: base(underbond, hAWB.MAWB)
		{
			this.HAWB = hAWB;
		}

		public override IUnderbondMovementRequestLine Line
		{
			get { return new CTOCusHAWBUnderbondMovementRequestLine(HAWB); }
		}

		protected readonly CTOCusHAWB HAWB;
	}
}
