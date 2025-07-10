using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public static class MessagingMenuExtension
	{
		public static string MessageHasBeenSent => Res.GetString("e971ed06-900f-4d91-9db5-c855ec673d7b", "The message has been sent.");

		public static void SetCaptionResourceString(this ZMenuItem menu)
		{
			menu.CaptionResourceString = Res.GetData("736b9eb3-3e04-4b49-89e1-8b93991d51b9", "&Messages");
		}

		public static bool PreSaveMessage(this KMenuItem menu, CusTempStorageJobHeader header)
		{
			var result = true;
			if (header.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("8e9d6472-7f56-4fee-9cbc-439501ffa677", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("51f8291e-20a6-405d-b9ba-8b42191744ab", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes);
				result = messageBoxResult == DialogResult.Yes && ((ZForm)menu.GetMainMenu()?.GetForm()).FireSaveButton() == ContinueWithSave.Yes;
			}
			return result && !header.HasChanges;
		}

		public static bool CheckDeclarationStatusAndLines(this CusTempStorageDec cusTempStorageDec)
		{
			var notifications = new NotificationCollection();

			if (cusTempStorageDec == null)
			{
				notifications.AddError(Res.GetString("717A7674-E80E-4084-B663-793FD8A56AEC", "No Declaration has been created."));
			}
			else
			{
				notifications.AddErrorIf(cusTempStorageDec.STH_MessageStatus == Common.Shared.MessageStatusList.Codes.Sent
					, Res.GetString("C05D27E4-4F48-4250-8478-DA1943EAEA44", "Cannot send further messages, still awaiting a response."));
				notifications.AddErrorIf(!cusTempStorageDec.CusTempStorageLines.Any()
					, Res.GetString("EBD2ADCA-DE03-450C-BE27-C0498C9D6F28", "Declaration has no lines, message cannot be created."));
			}

			var hasErrors = notifications.HasErrors();
			if (hasErrors)
			{
				Globals.Message.ShowError(notifications.ToMessageListString());
			}
			return !hasErrors;
		}

		static void AddErrorIf(this NotificationCollection notifications, bool hit, string message)
		{
			if (hit)
			{
				notifications?.AddError(message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Confirmation String")]
		public static bool CanSendMessage(this CusTempStorageJobHeader header, CusTempStorageDec cusTempStorageDec)
		{
			var canSend = cusTempStorageDec.CheckDeclarationStatusAndLines();
			if (canSend)
			{
				header.RunPreSaveValidation();
				var messageErrors = header.GetHeaderAndTempStorageDecMessageErrors(cusTempStorageDec);
				if (messageErrors.HasMessageErrors())
				{
					if (Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed)
					{
						var warningForSuperUser = Res.GetString("E11A90CF-D6DE-401F-9999-66E1949C7074", "It is likely that your message(s) will be rejected as they have the following message errors:") + System.Environment.NewLine + System.Environment.NewLine
												+ messageErrors.ToMessageListString() + System.Environment.NewLine
												+ Res.GetString("5D90A146-130B-4E02-86D0-1416BD8A7B7D", "Do you want to send the message(s) despite these message errors?");
						canSend = Globals.Message.ShowConfirmation(warningForSuperUser, Res.GetString("8171142D-72DC-41D4-9AD7-ABB35F7E55B4", "Send Message"), "Yes", MessageBoxIcon.Question) == DialogResult.OK;
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("563B1872-7BC7-4A08-98B4-D0904600AEEA", "Please fix the following message errors before sending any messages.") + System.Environment.NewLine + System.Environment.NewLine + messageErrors.ToMessageListString());
						canSend = false;
					}
				}
			}
			return canSend;
		}
	}
}
