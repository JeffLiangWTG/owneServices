using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class ACIHouseBillMultiMessageManager : MultiMessageManager
	{
		public ACIHouseBillMultiMessageManager(GetMaserDelegate getMasterDelegate)
		{
			this.getMasterDelegate = getMasterDelegate;
		}
		readonly GetMaserDelegate getMasterDelegate;

		public delegate CusCAeMHMaster GetMaserDelegate();

		public override IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return Master; }
		}

		public CusCAeMHMaster Master
		{
			get { return getMasterDelegate == null ? null : getMasterDelegate(); }
		}

		public void RefreshAll(IConsolSendsMessagesToCustoms sender)
		{
			if (sender != null)
			{
				SingleMessageManager[] messageManagersToReset = sender.WhichMessagesShouldWeRefresh(AllMessageManagers);
				if (messageManagersToReset.Length > 0)
				{
					if (sender.ShowUserConfirmation(RefreshDataComments, RefreshAllCaption, RefreshAllConfirmationMessage + " ", "REFRESH"))
					{
						foreach (CAEManifestForwarderMessageManager messageManager in messageManagersToReset)
						{
							messageManager.RefreshDetails();
						}
					}
				}
			}
		}

		static string RefreshDataComments
		{
			get
			{
				return Res.GetString("6F7EEBCB-5E57-4CF3-A6C3-EA97B1F509DA", @"Warning - You are about to refresh ACI eManifest Forwarder data!
Data entered on the eManifest Forwarder tab for selected shipment(s) will be refreshed from original shipment data. Any data that has been overridden on the this tab may be lost. You should check the data on eManifest House Bill tab of selected shipments. If House Bill Reports have already been submitted to Customs then you should re-send messages, after the refresh, to ensure that correct data has been reported.");
			}
		}

		static string RefreshAllCaption
		{
			get { return Res.GetString("ABB59FD6-55A5-4EEA-A315-AC3E3AF7C107", "Warning - Refresh data?"); }
		}

		static string RefreshAllConfirmationMessage
		{
			get { return Res.GetString("002A4454-C21B-492F-B6BD-5682C69CF002", "If you are sure you want to refresh eManifest Forwarder data, please type:"); }
		}

		#region Implementation

		public override ZString MessagingApplicationName
		{
			get { return "eManifest Fwdr"; }
		}

		protected override void OnMessagesSending(SingleMessageManager[] singleMessageManagers)
		{
			base.OnMessagesSending(singleMessageManagers);
			if (OnMessagesSendingEvent != null)
			{
				OnMessagesSendingEvent(singleMessageManagers);
			}
		}

		public event OnMessagesSendingDelegate OnMessagesSendingEvent;
		public delegate void OnMessagesSendingDelegate(SingleMessageManager[] singleMessageManagers);

		protected override string OriginalMessageTypeUsedInConfirmation
		{
			get { return string.Empty; }
		}

		protected override string AmendmentMessageTypeUsedInConfirmation
		{
			get { return string.Empty; }
		}

		protected override string WithdrawalMessageTypeUsedInConfirmation
		{
			get { return Res.GetString("22F9CA5E-E2C0-47CF-B966-8EEF67B6E7CB", "cancel"); }
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			if (Master != null)
			{
				foreach (CusCAeMHHouse house in Master.HouseBills)
				{
					var singleManager = new ACIHouseBillMessageManager(house, null);
					result.Add(singleManager);
				}
			}
			return result.ToArray();
		}

		public override IList<Enterprise.Messaging.Business.EDIMessage> SendOriginalMessages(ISendsMessagesToCustoms sender)
		{
			using (Master.SetIsValidatingAll())
			{
				UpdateSenderActionPurpose(sender, ActionPurpose.Origin);
				return base.SendOriginalMessages(sender);
			}
		}

		public bool SendOriginalMessages(ISendsMessagesToCustoms sender, ACIHouseBillMessageManager[] messagesToSend)
		{
			using (Master.SetIsValidatingAll())
			{
				Initialise();
				return SendOriginal(sender, messagesToSend).Any();
			}
		}

		public bool SendCloseMessages(ISendsMessagesToCustoms sender, bool isForced = false, bool isAmendment = false)
		{
			UpdateSenderActionPurpose(sender, ActionPurpose.Origin);
			var result = false;
			Initialise();
			if (Master != null)
			{
				var closeWrapper = GetCloseWrapper(isForced);
				var messagesToSendManagers = new SingleMessageManager[] { new ACIForwarderCloseMessageManager(closeWrapper, null, isForced) };
				if (!isForced)
				{
					var acceptedCCNsAsString = GetAcceptedCCNsFromCloseWrapper(closeWrapper);
					if (!acceptedCCNsAsString.IsEmpty)
					{
						sender.WarnUserAboutSomething(GetSendCloseMessagesMessage(acceptedCCNsAsString), SendCloseMessagesCaption);
					}
					result = SendOriginal(sender, messagesToSendManagers).Any();
				}
				else
				{
					if (isAmendment)
					{
						UpdateSenderActionPurpose(sender, ActionPurpose.Change);
					}
					result = SendOriginal(sender, sender.WhichMessagesShouldWeSend(messagesToSendManagers)).Any();
				}
			}
			return result;
		}

#if DEBUG
		public
#endif
		CusCAeMHMasterCloseWrapper GetCloseWrapper(bool isForced = false)
		{
			CusCAeMHMasterCloseWrapper result = null;
			if (Master != null)
			{
				var closeWrapper = new CusCAeMHMasterCloseWrapper(Master);
				closeWrapper.UpdateIsShouldSend(house => !isForced && house.IsAccepted);
				result = closeWrapper;
			}
			return result;
		}

#if DEBUG
		public
#endif
		ZString GetAcceptedCCNsFromCloseWrapper(CusCAeMHMasterCloseWrapper closeWrapper)
		{
			return ZString.Join(", ", closeWrapper?.RelatedCCNs?.ToArray());
		}

		static string SendCloseMessagesCaption
		{
			get { return Res.GetString("600BCE15-3CCD-434F-8B50-8C7CF97DF4ED", "Send CCN close messages"); }
		}

		string GetSendCloseMessagesMessage(ZString acceptedCCNsAsString)
		{
			return Res.GetString("F7BF7872-D1B2-48E3-98E6-B619BE6237A0", "You are about to close below CCNs: {0}", acceptedCCNsAsString);
		}

		public override bool WithdrawMessages(ISendsMessagesToCustoms sender)
		{
			UpdateSenderActionPurpose(sender, ActionPurpose.Withdraw);
			return base.WithdrawMessages(sender);
		}

		public bool WithdrawCloseMessages(ISendsMessagesToCustoms sender, bool isForced = true)
		{
			UpdateSenderActionPurpose(sender, ActionPurpose.Withdraw);
			var result = false;
			Initialise();
			if (Master != null)
			{
				var closeWrapper = GetCloseWrapper(true);
				var messagesToSendManagers = new SingleMessageManager[] { new ACIForwarderCloseMessageManager(closeWrapper, null, isForced) };
				result = Withdraw(sender, messagesToSendManagers);
			}
			return result;
		}

		protected override void ResetToOriginalCore(ISendsMessagesToCustoms sender)
		{
			UpdateSenderActionPurpose(sender, ActionPurpose.Change);
			base.ResetToOriginalCore(sender);
		}

		void UpdateSenderActionPurpose(ISendsMessagesToCustoms sender, ActionPurpose actionPurpose)
		{
			var caSender = sender as ICASendsMessagesToCustoms;
			if (caSender != null)
			{
				caSender.FormActionPurpose = actionPurpose;
			}
		}

		protected override SecurityCheckpoint ResetToOriginalSecurityCheckpoint
		{
			get
			{
				return Env.Security.ConsolCAeManifestResetToOriginal;
			}
		}

		protected override SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint
		{
			get
			{
				return Env.Security.ConsolCAeManifestSendWithMessageErrors;
			}
		}

		#endregion
	}
}
