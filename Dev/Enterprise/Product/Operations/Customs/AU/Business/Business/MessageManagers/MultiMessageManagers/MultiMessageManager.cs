using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for MultiMessageManager.
	/// </summary>
	public abstract class MultiMessageManager : Customs.Business.MultiMessageManager
	{
		#region Notification

		protected override string ResetToOriginalWarning
		{
			get { return @"Warning - Resetting a message to original is almost never correct unless you have withdrawn the entry using the CI. Any reset to original will NOT ever work if the CI has not been used.
Do not reset messages because of a message problem or because you wish to try again - without using the CI to remove/withdraw/delete any current declaration or report.
If you reset to original incorrectly the system will not work correctly - cargo may be delayed, storage incurred or the system may become unusable for this entry / report."; }
		}

		public override bool ShouldSendMessagesInTestMode
		{
			get { return Env.Registry.CMRTestMode; }
		}

		#endregion Notification
	}
}
