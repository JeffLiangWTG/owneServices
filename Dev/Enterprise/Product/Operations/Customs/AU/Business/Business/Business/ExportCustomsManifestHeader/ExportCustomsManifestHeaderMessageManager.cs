using System;
using System.Collections.Specialized;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for ExportCustomsManifestHeaderMessageManager.
	/// </summary>
	public class ExportCustomsManifestHeaderMessageManager : Customs.Business.MessageManager
	{
		public ExportCustomsManifestHeaderMessageManager(ExportCustomsManifestHeader header)
			: base(false)
		{
			this.header = header;
		}

		#region Overridden Stuff

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates
		{
			get { return new SendAmendmentIfNeededDelegate[] { new SendAmendmentIfNeededDelegate(SendADepartureReportAmendmentIfNeeded), new SendAmendmentIfNeededDelegate(SendAManifestAmendmentIfNeeded) }; }
		}

		protected override BusinessObject Master
		{
			get { return header; }
		}

		#endregion

		#region Message Send Errors and Warnings

		internal StringCollection GetCommonReasonForNotSendingAMessage()
		{
			return CMRMessageManager.CheckCMRMessageSendingEnvironment(header.Factory);
		}

		protected virtual StringCollection GetAnyReasonsWeCantDeclareTheDepartureReport()
		{
			var result = GetCommonReasonForNotSendingADepartureReportMessage();
			if (header.NotificationsIncludingChildren.HasErrors())
			{
				foreach (var error in header.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
				{
					result.Add(error);
				}
			}
			if (header.IsDepartureReportDeclaredAtCustoms)
			{
				result.Add("You can't send a departure report original because you already have a report declared at Customs");
			}

			return result;
		}

		protected virtual StringCollection GetAnyReasonsWeCantReplaceTheDepartureReport()
		{
			return GetCommonReasonForNotSendingADepartureReportMessage();
		}

		protected virtual StringCollection GetAnyReasonsWeCantWithdrawTheDepartureReport()
		{
			var result = GetCommonReasonForNotSendingADepartureReportMessage();
			if (header.HasChanges)
			{
				result.Add("You may not withdraw the departure report because you have unsaved changes");
			}

			if (!header.IsDepartureReportDeclaredAtCustoms)
			{
				result.Add("You can't send a departure report withdrawal because the report is not currently declared at Customs");
			}

			return result;
		}

		protected virtual internal StringCollection GetCommonReasonForNotSendingADepartureReportMessage()
		{
			var result = GetCommonReasonForNotSendingAMessage();
			if (header.IsWaitingForDepartureReportResponse)
			{
				result.Add("The system is currently waiting for a departure report response from Customs.");
			}

			if (header.IsDeparture && header.IsSea)
			{
				if (header.CarrierPartyID.IsEmpty)
				{
					result.Add("An ABN or CCID is required for the current company if sending sea departure reports.");
				}
			}
			return result;
		}

		protected virtual StringCollection GetWarningsAboutDeclaringTheDepartureReport()
		{
			var result = new StringCollection();
			if (header.NotificationsIncludingChildren.HasMessageErrors())
			{
				foreach (var messageError in header.NotificationsIncludingChildren.GetMessageErrors().GetUniqueMessageList())
				{
					result.Add(messageError);
				}
			}
			return result;
		}

		protected virtual StringCollection GetWarningsAboutWithdrawingTheDepartureReport()
		{
			return new StringCollection();
		}

		protected virtual internal StringCollection GetAnyReasonsWeCantDeclareTheManifest()
		{
			var result = GetCommReasonForNotSendingAManifestMessage();

			try
			{
				if (header.NotificationsIncludingChildren.GetErrors().HasNotifications())
				{
					foreach (var error in header.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
					{
						result.Add(error);
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				ErrorReporter.ReportOnce("GetAnyReasonsWeCantDeclareTheManifest InvalidOperationException", "GetAnyReasonsWeCantDeclareTheManifest InvalidOperationException", ex);
			}

			if (header.IsManifestDeclaredAtCustoms)
			{
				result.Add("You can't send a manifest original because you already have a CAN assigned by Customs");
			}

			return result;
		}

		protected virtual StringCollection GetAnyReasonsWeCantReplaceTheManifest()
		{
			return GetCommReasonForNotSendingAManifestMessage();
		}

		protected virtual internal StringCollection GetAnyReasonsWeCantWithdrawTheManifest()
		{
			var result = GetCommReasonForNotSendingAManifestMessage();
			if (header.HasChanges)
			{
				result.Add("You may not withdraw the manifest because you have unsaved changes");
			}

			if (!header.IsManifestDeclaredAtCustoms)
			{
				result.Add("You can't send a manifest withdrawal because you have not yet been issued a CAN for this manifest");
			}

			return result;
		}

		protected virtual StringCollection GetCommReasonForNotSendingAManifestMessage()
		{
			var result = GetCommonReasonForNotSendingAMessage();
			if (header.IsWaitingForManifestResponse)
			{
				result.Add("The system is currently waiting for a manifest response from Customs.");
			}

			return result;
		}

		protected virtual StringCollection GetWarningsAboutDeclaringTheManifest()
		{
			var result = new StringCollection();
			if (header.NotificationsIncludingChildren.GetMessageErrors().HasNotifications())
			{
				foreach (var messageError in header.NotificationsIncludingChildren.GetMessageErrors().GetUniqueMessageList())
				{
					result.Add(messageError);
				}
			}
			return result;
		}

		protected virtual internal StringCollection GetWarningsAboutWithdrawingTheManifest()
		{
			var result = new StringCollection();
			if (header.Lines.Count > 0)
			{
				result.Add("You should only withdraw manifest that have zero lines.");
			}

			return result;
		}

		#endregion

		#region Message Sending

#if DEBUG

		public void DeclareManifest(Customs.Business.ISendsMessagesToCustoms sender)
		{
			DeclareManifest(sender, CancellationToken.None);
		}

		public void WithdrawManifest(Customs.Business.ISendsMessagesToCustoms sender)
		{
			WithdrawManifest(sender, CancellationToken.None);
		}

#endif

		public void DeclareManifest(Customs.Business.ISendsMessagesToCustoms sender, CancellationToken token)
		{
			SendMessage(sender, GetAnyReasonsWeCantDeclareTheManifest(), GetWarningsAboutDeclaringTheManifest(), new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetOriginalManifestBuilder) }, "Manifest declaration", token);
		}

		public void ResetToOriginal(Customs.Business.ISendsMessagesToCustoms sender)
		{
			ResetManifest(sender);
		}

		public void WithdrawManifest(Customs.Business.ISendsMessagesToCustoms sender, CancellationToken token)
		{
			SendMessage(sender, GetAnyReasonsWeCantWithdrawTheManifest(), GetWarningsAboutWithdrawingTheManifest(), new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetReplacementZeroLineManifestBuilder), new MessageBuilderDelegate(GetPendingWithdrawalManifestBuilder) }, "Manifest withdrawal", token);
		}

		public void DeclareDepartureReport(Customs.Business.ISendsMessagesToCustoms sender, CancellationToken token)
		{
			SendMessage(sender, GetAnyReasonsWeCantDeclareTheDepartureReport(), GetWarningsAboutDeclaringTheDepartureReport(), new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetOriginalDepartureReportBuilder) }, "Departure Report declaration", token);
		}

		public void WithdrawDepartureReport(Customs.Business.ISendsMessagesToCustoms sender, CancellationToken token)
		{
			SendMessage(sender, GetAnyReasonsWeCantWithdrawTheDepartureReport(), GetWarningsAboutWithdrawingTheDepartureReport(), new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetWithdrawalDepartureReportBuilder) }, "Departure Report withdrawal", token);
		}

		#endregion

		#region Amendment Sending Methods

		internal bool SendAManifestAmendmentIfNeeded(Customs.Business.ISendsMessagesToCustoms sender)
		{
			return SendAnAmendmentIfNeeded(sender, header.IsManifestDeclaredAtCustoms, header.IsWaitingForManifestResponse, new MessageBuilderDelegate(GetReplacementManifestBuilder), "Manifest", GetWarningsAboutDeclaringTheManifest(), GetAnyReasonsWeCantReplaceTheManifest());
		}

		protected bool SendADepartureReportAmendmentIfNeeded(Customs.Business.ISendsMessagesToCustoms sender)
		{
			return SendAnAmendmentIfNeeded(sender, header.IsDepartureReportDeclaredAtCustoms, header.IsWaitingForDepartureReportResponse, new MessageBuilderDelegate(GetReplacementDepartureReportBuilder), "Departure Report", GetWarningsAboutDeclaringTheDepartureReport(), GetAnyReasonsWeCantReplaceTheDepartureReport());
		}

		#endregion

		protected virtual void ResetManifest(Customs.Business.ISendsMessagesToCustoms sender)
		{
			if (CanReset(sender))
			{
				header.Messages.UpdateStatusOfAllMessagesTo(EDIMessage.Status.Discarded);
			}
		}

		bool CanReset(Customs.Business.ISendsMessagesToCustoms sender)
		{
			if (Env.Security.CustomsResetToOriginal.IsAllowed)
			{
				return sender.ShowUserConfirmation("Resetting to original may have undesired consequences. Proceed with caution.", "Warning - reset to original", "If you are sure you want to reset, type: ", "reset");
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsResetToOriginal);
				return false;
			}
		}

		#region Message Builders

		internal IMessageBuilder GetOriginalManifestBuilder(BusinessObject master)
		{
			var header = master as ExportCustomsManifestHeader;
			if (header.IsMainManifest)
			{
				var builder = new EMMMessageBuilder(header);
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
				return builder;
			}
			else
			{
				var builder = new ESMMessageBuilder(header, header.IsSlot);
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
				return builder;
			}
		}

		internal IMessageBuilder GetReplacementManifestBuilder(BusinessObject master)
		{
			var header = master as ExportCustomsManifestHeader;
			CMRManifestMessageBuilder result;
			if (header.IsMainManifest)
			{
				result = new EMMMessageBuilder(header);
			}
			else
			{
				result = new ESMMessageBuilder(header, header.IsSlot);
			}
			result.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			return result;
		}

		protected IMessageBuilder GetReplacementZeroLineManifestBuilder(BusinessObject master)
		{
			var header = master as ExportCustomsManifestHeader;
			CMRManifestMessageBuilder result;
			if (header.IsMainManifest)
			{
				result = new EMMMessageBuilder(header);
			}
			else
			{
				result = new ESMMessageBuilder(header, header.IsSlot);
			}
			result.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			result.DontSendAnyLines = true;
			return result;
		}

		internal IMessageBuilder GetWithdrawalManifestBuilder(BusinessObject master)
		{
			var header = master as ExportCustomsManifestHeader;
			if (header.IsMainManifest)
			{
				var builder = new EMMMessageBuilder(header);
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
				return builder;
			}
			else
			{
				var builder = new ESMMessageBuilder(header, header.IsSlot);
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
				return builder;
			}
		}

		protected IMessageBuilder GetPendingWithdrawalManifestBuilder(BusinessObject master)
		{
			var result = GetWithdrawalManifestBuilder(master);
			if (result is CMRManifestMessageBuilder)
			{
				(result as CMRManifestMessageBuilder).SetStatusToPending = true;
			}
			return result;
		}

		protected IMessageBuilder GetOriginalDepartureReportBuilder(BusinessObject master)
		{
			var header = master as ExportCustomsManifestHeader;
			var builder = new DEPARTMessageBuilder(header);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			return builder;
		}

		protected IMessageBuilder GetWithdrawalDepartureReportBuilder(BusinessObject master)
		{
			var header = master as ExportCustomsManifestHeader;
			var builder = new DEPARTMessageBuilder(header);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			return builder;
		}

		protected IMessageBuilder GetReplacementDepartureReportBuilder(BusinessObject master)
		{
			var header = master as ExportCustomsManifestHeader;
			var builder = new DEPARTMessageBuilder(header);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			return builder;
		}

		#endregion

		#region Implementation

		protected override bool ShouldSendMessagesInTestMode
		{
			get { return Env.Registry.CMRTestMode; }
		}

		internal ExportCustomsManifestHeader header;

		#endregion
	}
}
