using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class MessageSender : IMessageSender<MessageSender.Parameters>
	{
		public MessageSender(IMessageInstructions instructions, IDocument document, IVisualizerDocumentData documentData, IServiceContainer services)
		{
			Argument.NotNull(instructions, nameof(instructions));
			Argument.NotNull(document, nameof(document));
			Argument.NotNull(documentData, nameof(documentData));
			Argument.NotNull(services, nameof(services));

			this.instructions = instructions;
			this.document = document;
			this.documentData = documentData;
			this.deliveryService = services.Resolve<IDocumentDeliveryService>();
			this.notificationService = services.Resolve<IUserNotificationService>();
			this.eventBroker = services.Resolve<IEventBroker>();
		}

		readonly IMessageInstructions instructions;
		readonly IDocument document;
		readonly IVisualizerDocumentData documentData;
		readonly IDocumentDeliveryService deliveryService;
		readonly IUserNotificationService notificationService;
		readonly IEventBroker eventBroker;

		string Caption => Res.GetString("28db12fa-d303-43a9-b599-d6fa2c2ba9a7", "Sending Message");

		public bool Send(Parameters parameters)
		{
			if (!IsValid(parameters))
			{
				return false;
			}

			var sendingAmendment = CalculateDataVersion() > 1;

			var isAmendmentReasonRequired = sendingAmendment
				&& GetIsAmendmentReasonRequired();

			object amendmentReason = null;

			if (isAmendmentReasonRequired)
			{
				amendmentReason = GeAmendmentReason();

				if (amendmentReason is null)
				{
					return false;
				}
			}

			var notifications = new Business.NotificationsHandler();

			eventBroker.NotifyProgress(Res.GetString("299F8CAA-B57A-47D2-B6DE-50985DA26B57", "Sending message"));

			var result = SendUXml(notifications, amendmentReason);

			if (result)
			{
				eventBroker.Publish(new MessageSentEvent(document));
			}

			if (result
				|| notifications.Notifications.Any())
			{
				var messageText = notifications.Notifications.Any()
					? string.Join(System.Environment.NewLine, notifications.Notifications.Select(n => n.Message))
					: Res.GetString("bebb5ffe-2a9a-4089-8a41-4356404aa539", "Message has been sent.");

				notificationService.ShowMessage(messageText, Caption);
			}

			return result;
		}

		#region Validate

		bool IsValid(Parameters parameters)
		{
			var data = document.Data;

			if (data == null)
			{
				notificationService.ShowMessage(Res.GetString("2efe148e-3a3c-4900-9db9-1d46aa54b29d", "Document data could not be found."), Caption);
				return false;
			}

			if (instructions == null)
			{
				notificationService.ShowMessage(Res.GetString("d2ee6074-6cbc-4132-b001-0f3b8cbc99f1", "Message type must be selected prior to sending."), Caption);
				return false;
			}

			if (data.HasChanges)
			{
				notificationService.ShowMessage(Res.GetString("5e807eb9-5e00-4fc5-9310-759c55173277", "Please save all changes before sending."), Caption);
				return false;
			}

			if (document.HasErrors())
			{
				notificationService.ShowMessage(Res.GetString("1519010e-e5a8-40e7-b7b1-17bc60bfc3df", "This document contains errors. Please fix all errors before sending."), Caption);
				return false;
			}

			if (document.HasMessageErrors())
			{
				notificationService.ShowMessage(Res.GetString("3d1ddf18-2a01-434b-bb15-871fb1f567e0", "This document contains message errors. Please fix all message errors before sending."), Caption);
				return false;
			}

			var bizObj = documentData.Parent as BusinessObject;

			var messageExtensions = bizObj
					?.GetSupporter()
					?.GetMessagingExtensions(document, instructions);

			var isSendingAmendment = messageExtensions?.IsSendingAmendment()
				?? MesageHasBeenSent;

			if (messageExtensions != null)
			{
				var res = isSendingAmendment
					? messageExtensions.ContinueWithSendingMessageAmendment(notificationService)
					: messageExtensions.ContinueWithSendingMessage(notificationService);

				if (res.HasValue)
				{
					return res.Value;
				}
			}

			var lastDialog = documentData
				.GetDialogs(instructions.DocumentName, instructions.OrderLogsByLocalTime)
				.LastOrDefault();

			if (!instructions.AllowSendMessageAmendment && isSendingAmendment)
			{
				notificationService.ShowMessage(Res.GetString("EB9EAF6F-BE16-43C0-B510-36C0041189C9", "A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option."), Res.GetString("7aa4626e-3e0c-47f0-89e6-c084c9e8cc12", "Confirmation"));
				return false;
			}

			if (lastDialog != null && (SentMessageNoResponse(lastDialog) || ReceivedAcceptanceAmendmentConfirmationIsRequired(lastDialog, parameters)))
			{
				var captionWarn = Res.GetString("664d24f8-4833-476c-bc17-04610ee21d65", "Warning");
				var captionConfirm = Res.GetString("7aa4626e-3e0c-47f0-89e6-c084c9e8cc12", "Confirmation");
				var promptWarn = Res.GetString("46aa1edc-df5a-4e3f-9ce5-43a5ae838cbe", "If you have done so, please type the following to confirm:");

				var confirmed = string.IsNullOrWhiteSpace(parameters.AmendmentConfirmation)
					? notificationService.ShowConfirmation(GetDefaultConfirmationMessage(), captionConfirm)
					: notificationService.ShowConfirmation(parameters.AmendmentWarning, captionWarn, promptWarn, parameters.AmendmentConfirmation);

				if (!confirmed)
				{
					return false;
				}
			}

			return true;
		}

		bool SentMessageNoResponse(IDialog lastDialog) => !lastDialog.HasReceivedResponse()
			&& MesageHasBeenSent;

		bool MesageHasBeenSent => documentData.CalculateDataVersion(instructions.DocumentName, instructions.OrderLogsByLocalTime) > 1;

		bool ReceivedAcceptanceAmendmentConfirmationIsRequired(IDialog lastDialog, Parameters parameters) => !string.IsNullOrWhiteSpace(parameters.AmendmentConfirmation)
			&& lastDialog.HasBeenAccepted();

		string GetDefaultConfirmationMessage()
		{
			var recipient = instructions.Recipient.IsNullOrEmpty()
				? Res.GetString("765005c3-7122-44d5-a2e4-18d8cb65855b", "recipient")
				: instructions.Recipient;

			var userOptionMessage = instructions.AllowResetToOriginal
				? Res.GetString("73ef8289-eb2e-4132-8e42-4a10d4b9c4ed", "You may want to reset the message to original first (using \"Reset to Original\" option) if the {0} did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?", recipient)
				: Res.GetString("ca6bb0fc-b9a0-4a34-8a70-ae1bd266e567", "Are you sure you want to send the message?");

			var confirmationTemplate = Res.GetString("a2ae1d7a-ef60-40b2-9923-1195887200b9", @"A message has previously been sent, and there is no reply received from the {0}. Resending the message may cause errors and possibly revert to a manual process.", recipient);

			return confirmationTemplate + System.Environment.NewLine + System.Environment.NewLine + userOptionMessage;
		}

		#endregion

		#region SendUXml

		bool SendUXml(CargoWise.ComponentModel.INotifications notifications, object amendmentReason)
		{
			using (PerformanceStatistics.StartMonitoring(PerformanceMonitorArea.SendUniversalXml))
			{
				var dataVersion = documentData.CalculateDataVersion(instructions.DocumentName, instructions.OrderLogsByLocalTime);
				var messageType = dataVersion > 1
					? MessageType.Amendment
					: MessageType.Original;

				var parameters = new UXmlSender.Parameters
				{
					Instructions = instructions,
					Document = document,
					DocumentData = documentData,
					DeliveryService = deliveryService,
					MessageType = messageType,
					ReasonForSending = amendmentReason
				};

				var sender = new UXmlSender(parameters);

				return sender.Send(notifications);
			}
		}

		#endregion

		#region AttachReasonForMessageAmendmentNoteIfNecessary

		bool GetIsAmendmentReasonRequired()
		{
			if (documentData.Parent is BusinessObject bizObj)
			{
				var messageExtensions = bizObj
					.GetSupporter()?
					.GetMessagingExtensions(document, instructions);

				var res = messageExtensions?.GetRequireMessageAmendmentReason();

				if (res.HasValue)
				{
					return res.Value;
				}
			}

			return instructions.RequireMessageAmendmentReason;
		}

		int CalculateDataVersion() => documentData.CalculateDataVersion(instructions.DocumentName, instructions.OrderLogsByLocalTime);

		ICodeDescriptionPairList GetAmendmentOptions()
		{
			if (documentData.Parent is BusinessObject bizObj)
			{
				var messageExtensions = bizObj
					.GetSupporter()?
					.GetMessagingExtensions(document, instructions);

				var res = messageExtensions?.GetAmendmentOptions();

				if (res != null)
				{
					return res;
				}
			}

			return instructions.AmendmentOptions;
		}

		object GeAmendmentReason()
		{
			if (documentData.Parent is BusinessObject bizObj)
			{
				var messageExtensions = bizObj
					.GetSupporter()?
					.GetMessagingExtensions(document, instructions);

				if (messageExtensions is ICustomMessageAmendmentSupporter customMessageAmendmentSupporter)
				{
					return customMessageAmendmentSupporter.GetMessageAmendmentReason();
				}
			}

			var optionsList = GetAmendmentOptions();

			var message = Res.GetString("c325a073-3822-41f5-a596-e1c353a43be3", "You are re-sending the {0} so this message acts as a replacement. Please enter a reason for this replacement:", document.Name);
			var caption = Res.GetString("a6ba74d8-9ec4-4364-9d96-06640694c63b", "Replacement Reason");

			if (optionsList != null)
			{
				var reasonCodeDescription = notificationService.QueryUserResponse(
					message,
					caption,
					optionsList);

				return reasonCodeDescription != null
					? string.Format(CultureInfo.InvariantCulture, "{0} - {1}", reasonCodeDescription.Code, reasonCodeDescription.Description)
					: null;
			}

			var userResponse = notificationService.QueryUserResponse(
				message,
				caption,
				2,
				100);

			return !string.IsNullOrWhiteSpace(userResponse)
				? userResponse
				: null;
		}

		#endregion

		#region Nested Types

		public struct Parameters
		{
			public static Parameters New(MacroMap map)
			{
				var parameters = new Parameters();

				if (map != null)
				{
					parameters.AmendmentWarning = map.ContainsKey(nameof(AmendmentWarning))
						? Convert.ToString(map[nameof(AmendmentWarning)], CultureInfo.InvariantCulture)
						: string.Empty;

					parameters.AmendmentConfirmation = map.ContainsKey(nameof(AmendmentConfirmation))
						? Convert.ToString(map[nameof(AmendmentConfirmation)], CultureInfo.InvariantCulture)
						: string.Empty;
				}

				return parameters;
			}

			public string AmendmentWarning { get; private set; }
			public string AmendmentConfirmation { get; private set; }
		}

		#endregion
	}
}
