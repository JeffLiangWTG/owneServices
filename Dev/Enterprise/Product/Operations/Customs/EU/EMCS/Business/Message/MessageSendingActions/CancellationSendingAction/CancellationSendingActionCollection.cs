namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class CancellationSendingActionCollection : EMCSMessageSendingActionCollection<CancellationSendingAction>
	{
		public CancellationSendingActionCollection(CancellationSendingActionParent sendingParent) : base(sendingParent)
		{
		}
	}
}
