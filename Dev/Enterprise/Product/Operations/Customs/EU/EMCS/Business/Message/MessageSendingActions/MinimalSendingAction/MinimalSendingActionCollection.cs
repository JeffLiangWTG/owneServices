namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class MinimalSendingActionCollection : EMCSMessageSendingActionCollection<EMCSMessageSendingAction>
	{
		public MinimalSendingActionCollection(MinimalSendingActionParent sendingActionParent) : base(sendingActionParent) { }

		protected override EMCSMessageSendingAction CreateElementCore(EMCSJobDeclaration jobDeclaration)
		{
			var sendingAction = base.CreateElementCore(jobDeclaration);
			return sendingAction;
		}
	}
}
