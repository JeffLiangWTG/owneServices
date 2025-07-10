namespace Enterprise.Customs.CA.Business
{
	public enum MessageType { Undefined, DataLoadingModule, G7ExportDeclaration, EDIRelease, B3Cusdec }

	public class JobDeclarationMessageManager : Customs.Business.MultiMessageManager
		, Customs.Business.IMessageManager
	{
		public JobDeclarationMessageManager(JobDeclaration declaration, CAMessageSendingActionCollection actions)
		{
			this.declaration = declaration;
			this.actions = actions;
		}

		public readonly CAMessageSendingActionCollection actions;

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return declaration; }
		}

		public bool MergeIfNecessary(Customs.Business.ISendsMessagesToCustoms sender)
		{
			bool result = true;
			if (declaration.MergeManager.RequiresMerge || declaration.ActiveEntryHeaders.Count == 0)
			{
				result = declaration.DoMerge(sender);
			}
			return result;
		}

		#region Implementation
		protected readonly JobDeclaration declaration;

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return true; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			return actions.GetSingleMessageManagersSelected();
		}

		protected override GetAllSingleMessageManagersDelegate GetAllSingleMessageManagersDelegateForAmendmentDetection
		{
			get { return delegate { return actions.GetAllSingleMessageManagers(); }; }
		}

		protected override bool ShouldWaitUntilResponded
		{
			get { return false; }
		}
		#endregion Implementation

		#region IMessageManager Members

		Customs.Business.MessageSendingNotificationCollection Customs.Business.IMessageManager.CheckBusinessObjectLevelValidationIfRequired()
		{
			return Customs.Business.MessageSendingValidation.New(declaration, null).CheckBusinessObjectLevelValidation();
		}

		Customs.Business.IDeferredAmendmentSavingOptions Customs.Business.IMessageManager.GetDeferredAmendmentSavingOptions()
		{
			return actions;
		}

		protected override void ProcessWhenChangesAreSavedWithoutSendingCore(Customs.Business.IDeferredAmendmentSavingOptions saveOptions, Customs.Business.RequiredMessagesInformation information)
		{
			actions?.ProcessWhenChangesAreSavedWithoutSending();
		}

		#endregion
	}
}
