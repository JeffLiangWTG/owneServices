using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public static class DeferredB3PostSaveSendingExtension
	{
		public static ContinueWithSave AskForB3ActionIfDeferredMessageExists(this JobDeclaration declaration)
		{
			var result = ContinueWithSave.Yes;
			var entryHeader = declaration?.B3EntryHeader;
			if (entryHeader != null)
			{
				entryHeader.NeedCancelDeferredB3CADMessage = false;
				entryHeader.NeedResendDeferredB3CADMessage = false;
				entryHeader.NeedResendB3CADMessageSilent = false;
				entryHeader.OriginalDeferredB3CADMessageTime = ZDateTime.Empty;

				var existingDeferredB3Time = declaration == null ? ZDateTime.Empty : declaration.ScheduledB3MessageTime;
				if (existingDeferredB3Time.IsValid)
				{
					var message = ResString.GetMultilingualString("BE7E91DE-A6EC-4359-8957-F06247EA8E9A", "You have made changes when there is a deferred Entry message to be sent at {0}. The previous message will be canceled and a new Entry message will be scheduled to replace it.", existingDeferredB3Time.ToString());
					var caption = ResString.GetMultilingualString("0a0b51e4-d151-455b-b9a4-a13f292d9652", "Re-fresh and send Entry messages");

					Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);

					entryHeader.NeedCancelDeferredB3CADMessage = true;
					entryHeader.NeedResendDeferredB3CADMessage = true;
					entryHeader.NeedResendB3CADMessageSilent = true;
					entryHeader.OriginalDeferredB3CADMessageTime = entryHeader.DeferredB3MessageTime;
				}
			}
			return result;
		}

		public static void ResendDeferredMessageIfRequired(this JobDeclaration declaration)
		{
			var entryHeader = declaration?.B3EntryHeader;
			if (entryHeader != null && entryHeader.NeedResendDeferredB3CADMessage)
			{
				SendMessage(declaration, entryHeader);

				entryHeader.NeedResendDeferredB3CADMessage = false;
				entryHeader.NeedResendB3CADMessageSilent = false;
				entryHeader.OriginalDeferredB3CADMessageTime = ZDateTime.Empty;
			}
		}

		public static void AskForMessageActionIfMVFEventPosted(this JobDeclaration declaration)
		{
			if (declaration != null && declaration.IsImport && !declaration.IsLVX
				&& declaration.JE_EntryAuthorisationDate.IsValid && declaration.CA_K84AccountingDate.IsEmpty)
			{
				var log = declaration.Logs.MostRecentLogByEventTime(Events.MessageValidationFailed);
				if (log != null && log.SL_PostedTimeUtc.IsValid)
				{
					var entryHeader = declaration.B3EntryHeader;
					if (entryHeader != null && !entryHeader.IsClearedB3CorCAD
						&& entryHeader.CH_Status != MessageStatusList.Codes.AwaitingOriginal
						&& Globals.Message.Show(
							ResString.GetMultilingualString("785a59a4-a9a4-4030-8c60-03e10da87083", "It appears that you are making changes to a declaration where an attempt to automatically send a CAD failed due to validation errors. Do you want to send the CAD message now?"),
							ResString.GetMultilingualString("334304c3-f7c1-401e-b2f2-bb92bf2033f6", "Do you want to send CAD messages?"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
					{
						SendMessage(declaration, entryHeader);
					}
				}
			}
		}

		static void SendMessage(JobDeclaration declaration, CusEntryHeader entryHeader)
		{
			if (entryHeader != null)
			{
				if (entryHeader.IsB3C)
				{
					var dataWrapper = declaration.IsLVS
						? (IB3Header)new LowValueShipmentsMessageWrapper(entryHeader)
						: new B3ImportMessageWrapper(entryHeader);
					var messageManager = new B3ImportMessageManager(dataWrapper, new MessageInstructionUserNotification());
					messageManager.SendMessage(MessageSubTypes.Create);
				}
				else if (entryHeader.IsCAD)
				{
					var wrapper = new CADMessageWrapper(entryHeader);
					new CADMessageManager(wrapper, new MessageInstructionUserNotification()).SendMessage();
				}
			}
		}
	}
}
