using System.ComponentModel;

namespace Enterprise.Client.UPE.Business
{
	public enum RebillFlags
	{
		Unflagged,
		IsChangedToFreeDomicile,
		IsTranshipment,
		IsAbandoned,
		IsRTS
	}

	public delegate void RebillFlagChangingEventHandler(RebillFlagChangingEventArgs args);

	public class RebillFlagChangingEventArgs : CancelEventArgs
	{
		public RebillFlagChangingEventArgs(RebillFlags rebillFlag)
		{
			this.RebillFlag = rebillFlag;
		}

		public readonly RebillFlags RebillFlag;
	}
}
