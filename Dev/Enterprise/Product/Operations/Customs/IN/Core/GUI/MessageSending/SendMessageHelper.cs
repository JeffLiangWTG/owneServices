using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public static class SendMessageHelper
{
	public static void GenerateMessage(IMessageSendingObjectParent sendingObjectParent, Func<SendMessageForm> getSendMessageForm)
	{
		var preSendingValidationError = new PreMessageSendingValidation().ValidateMessageSending();
		if (preSendingValidationError.IsEmpty)
		{
			preSendingValidationError = sendingObjectParent.ValidateBeforeSend();
		}

		if (preSendingValidationError.IsEmpty)
		{
			using var form = getSendMessageForm();
			if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
			{
				var context = form.Context;
				if (GetMessageDownloadPathIfRequired(context, out var messageDownloadFolder))
				{
					var messages = sendingObjectParent.SendAndSaveMessages(context);
					if (messages?.Length > 0)
					{
						var dowloadedFilePaths = DownloadMessagesIfRequired(context, messageDownloadFolder, messages);
						Globals.Message.Show(GetSuccessMessage(context, dowloadedFilePaths));
					}
					else
					{
						Globals.Message.ShowWarning(GetFailureMessage(context));
					}
				}
			}
		}
		else
		{
			Globals.Message.ShowError(preSendingValidationError);
		}
	}

	public static bool SetTokenPinIfNeeded(Form parentForm = null, MessageSendingContext context = MessageSendingContext.EMAIL)
	{
		if (!GlbStaff.CurrentUser.IsSupportUser)
		{
			var tokenPinStore = CertificateTokenPinStore.Instance as ITokenPinStore;
			if (tokenPinStore.GetPin().IsEmpty)
			{
				var userEnterableTokenPin = new UserEnterableTokenPin();
				if (ZFormModaliser.ShowDialogAndDispose(new EnterCryptokiCertificatePinForm(userEnterableTokenPin), parentForm) == DialogResult.OK)
				{
					tokenPinStore.SetPin(userEnterableTokenPin.Pin);
				}
				else
				{
					var cancelledCaption = context == MessageSendingContext.DOWNLOAD
												? Res.GetString("9739EBB4-74DF-44A7-A0A2-533882EBDA58", "Message downloading canceled.")
												: Res.GetString("FE9213ED-3266-4D62-A671-C0ED930885BC", "Message sending canceled.");
					Globals.Message.Show(cancelledCaption);
					return false;
				}
			}
		}

		return true;
	}

	static bool GetMessageDownloadPathIfRequired(MessageSendingContext context, out string messageDownloadFolder)
	{
		var result = true;
		messageDownloadFolder = null;

		if (context == MessageSendingContext.DOWNLOAD)
		{
			using var folderDialog = new ZFolderBrowserDialog();
			folderDialog.ShowNewFolderButton = true;
			folderDialog.RequireMappablePath = true;
			folderDialog.Description = Res.GetString("F3AE7939-BAF5-4F36-8A86-9ADFF2F8C918", "Please select a valid directory to download the message file.");
			result = ZFormModaliser.ShowCommonDialogWithoutDispose(folderDialog) == DialogResult.OK;
			if (result)
			{
				messageDownloadFolder = folderDialog.IsNeedingToUseEnterpriseChannel ? folderDialog.UnmappedSelectedPath : folderDialog.MappedSelectedPath;
			}
		}
		return result;
	}

	static ZString DownloadMessagesIfRequired(MessageSendingContext context, string messageDownloadFolder, EDIMessage[] messages)
	{
		var dowloadedFilePaths = ZString.Empty;
		if (context == MessageSendingContext.DOWNLOAD)
		{
			var downloadedFilesPathBuilder = new ZStringBuilder();
			messages.ForEach(m => downloadedFilesPathBuilder.Append(CustomsMessageExporter.SaveToFile(m, messageDownloadFolder)));
			dowloadedFilePaths = downloadedFilesPathBuilder.ToStringWithNewLineBetweenAppends();
		}
		return dowloadedFilePaths;
	}

	static string GetSuccessMessage(MessageSendingContext context, ZString dowloadedFilePaths)
	{
		return context == MessageSendingContext.EMAIL
					? Res.GetString("3C3EEC17-5CA9-4A96-A8D9-94C459504906", "Message sent successfully.")
					: Res.GetString("B28525B3-39E1-4074-AF40-091C4B03D117", "Message downloaded successfully to\r\n{0}", dowloadedFilePaths);
	}

	static string GetFailureMessage(MessageSendingContext context)
	{
		return context == MessageSendingContext.EMAIL
					? Res.GetString("DF1357FA-CA93-41B4-96D3-E691A6F52713", "Message has not been sent. This may be caused by a failure or because sending has not been implemented.")
					: Res.GetString("DE4095B5-0C55-41CB-948D-C590304F3779", "Message has not been downloaded. This may be caused by a failure or because downloading has not been implemented.");
	}
}
