namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRShipmentStatuses : CMRStatuses
	{
		public CMRShipmentStatuses()
		{
			this.RemoveCode(CMRBaseStatuses.Codes.NotSent);
			this.AddPair(CMRBaseStatuses.Codes.NotSent, ResString.GetMultilingualString("CMRShipmentStatuses|NotSent", "No customs status received"));
		}
	}
}
