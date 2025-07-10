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
	public sealed class WithdrawalMessageSender : IMessageSender<WithdrawalMessageSender.Parameters>
	{
		public WithdrawalMessageSender(IMessageInstructions instructions, IDocument document, IVisualizerDocumentData documentData, IServiceContainer services)
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

		string Caption => Res.GetString("4a61a2d5-8b53-43be-aa98-46002d238dce", "Sending Withdraw/Cancel Request");

		public bool Send(Parameters parameters)
		{
			var dialogs = documentData
				.GetDialogs(instructions.DocumentName, instructions.OrderLogsByLocalTime)
				.ToArray();

			if (!IsValid(dialogs))
			{
				return false;
			}

			var withdrawalReason = GetWithdrawalReason();

			if (withdrawalReason is null)
			{
				return false;
			}

			var notifications = new Business.NotificationsHandler();

			var result = SendUXml(notifications, withdrawalReason);

			if (result)
			{
				eventBroker.Publish(new MessageWithdrawalSentEvent(document));
			}

			if (result
				|| notifications.Notifications.Any())
			{
				var messageText = notifications.Notifications.Any()
					? string.Join(System.Environment.NewLine, notifications.Notifications.Select(n => n.Message))
					: Res.GetString("cc0a4dfb-c29a-45b4-b0e7-034a20e15aa7", "Message withdrawal has been sent.");

				notificationService.ShowMessage(messageText, Caption);
			}

			return result;
		}

		#region Validate

		bool IsValid(IDialog[] dialogs)
		{
			if (documentData.Parent is BusinessObject bizObj)
			{
				var messageExtensions = bizObj
					.GetSupporter()?
					.GetMessagingExtensions(document, instructions);

				var res = messageExtensions?.ContinueWithSendingMessageWithdrawal(notificationService);

				if (res.HasValue)
				{
					return res.Value;
				}
			}

			if (!dialogs.Any())
			{
				notificationService.ShowMessage(Res.GetString("b0bb754a-747e-4b2f-a4d5-43668ee0d976", "There's no message to withdraw."), Caption);

				return false;
			}

			if (!dialogs.Last().HasReceivedResponse())
			{
				notificationService.ShowMessage(Res.GetString("f6253087-0dbd-47db-8641-28454c73e348", "Message can be withdrawn only after you have received a response to the previous message."), Caption);

				return false;
			}

			return true;
		}

		#endregion

		#region GetWithdrawalReason

		object GetWithdrawalReason()
		{
			if (documentData.Parent is BusinessObject bizObj)
			{
				var messageExtensions = bizObj
					.GetSupporter()?
					.GetMessagingExtensions(document, instructions);

				if (messageExtensions is ICustomMessageWithdrawalSupporter customMessageWithdrawalSupporter)
				{
					return customMessageWithdrawalSupporter.GetMessageWithdrawalReason();
				}
			}

			var optionsList = GetWithdrawalOptions();

			var message = Res.GetString("40e578da-f060-43e1-b80a-9b077150121e", "You are sending a {0} Cancellation message. Please enter a reason for cancellation:", document.Name);
			var caption = Res.GetString("eafad671-31ac-4b36-9944-4f025e98edab", "Cancellation Reason");

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

		ICodeDescriptionPairList GetWithdrawalOptions()
		{
			if (documentData.Parent is BusinessObject bizObj)
			{
				var messageExtensions = bizObj
					.GetSupporter()?
					.GetMessagingExtensions(document, instructions);

				var res = messageExtensions?.GetWithdrawalOptions();

				if (res != null)
				{
					return res;
				}
			}

			return instructions.WidthdrawalOptions;
		}

		#endregion

		#region SendUXml

		bool SendUXml(CargoWise.ComponentModel.INotifications notifications, object withdrawalReason)
		{
			using (PerformanceStatistics.StartMonitoring(PerformanceMonitorArea.SendUniversalXml))
			{
				var parameters = new UXmlSender.Parameters
				{
					Instructions = instructions,
					Document = document,
					DocumentData = documentData,
					DeliveryService = deliveryService,
					MessageType = MessageType.Withdrawal,
					ReasonForSending = withdrawalReason
				};

				var sender = new UXmlSender(parameters);

				return sender.Send(notifications);
			}
		}

		#endregion

		#region Nested Types

		public struct Parameters
		{
			public static Parameters New(MacroMap map)
			{
				var parameters = new Parameters();

				if (map != null && map.ContainsKey(SentCurrentDocumentDataName))
				{
					parameters.SentCurrentDocumentData = Convert.ToBoolean(map[SentCurrentDocumentDataName], CultureInfo.InvariantCulture);
				}

				return parameters;
			}

			public const string SentCurrentDocumentDataName = "SentCurrentDocumentData"; // MacroMap key constant

			public bool SentCurrentDocumentData { get; private set; }
		}

		#endregion
	}
}
