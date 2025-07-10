using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business.Presentation
{
	sealed class OriginalMessageSender : IMessageSender<OriginalMessageSender.Parameters>
	{
		public OriginalMessageSender(IMessageInstructions instructions, IDocument document, IVisualizerDocumentData dataStorage, IServiceContainer services, string documentName)
		{
			Argument.NotNull(services, nameof(services));

			this.broker = services.Resolve<IEventBroker>();
			this.document = Argument.NotNull(document, nameof(document));
			this.instructions = Argument.NotNull(instructions, nameof(instructions));
			this.notificationService = services.Resolve<IUserNotificationService>();
			this.dataStorage = Argument.NotNull(dataStorage, nameof(dataStorage));
			this.documentName = Argument.NotNull(documentName, nameof(documentName));
		}

		readonly IEventBroker broker;
		readonly IDocument document;
		readonly IMessageInstructions instructions;
		readonly IUserNotificationService notificationService;
		readonly IVisualizerDocumentData dataStorage;
		readonly string documentName;

		public bool Send(Parameters parameters)
		{
			if (!ContinueWithResetToOriginal(parameters))
			{
				return false;
			}

			var logParent = dataStorage as IStmALogParent;

			if (logParent == null)
			{
				return false;
			}

			if (!TryAddEventsFromSupporter(logParent))
			{
				AddStatusUpdatedEvent(logParent);
			}

			notificationService.ShowMessage(
				Res.GetString("31aa4598-c755-43c9-baa9-1b85f4937c99", "Reset to original successful."),
				Res.GetString("3f0ee7d0-b5ee-43e6-b22f-2a3eb7f43da8", "Reset To Original"));

			ZExceptionReporting.ProcessWithSaveExceptionHandling(logParent.Factory.Save, null);

			broker.Publish(new ResetToOriginalEvent(document));

			return true;
		}

		bool ContinueWithResetToOriginal(Parameters parameters)
		{
			if (dataStorage.Parent is BusinessObject docDataParent)
			{
				var supporter = docDataParent.GetSupporter();
				if (supporter?.GetMessagingExtensions(document, instructions) is IMessagingExtensions messagingExtensions)
				{
					var res = messagingExtensions.ContinueWithResetToOriginal(notificationService);

					if (res.HasValue)
					{
						return res.Value;
					}
				}
			}

			var caption = Res.GetString("664d24f8-4833-476c-bc17-04610ee21d65", "Warning");
			var prompt = Res.GetString("46aa1edc-df5a-4e3f-9ce5-43a5ae838cbe", "If you have done so, please type the following to confirm:");

			var confirmed = string.IsNullOrWhiteSpace(parameters.Confirmation)
				|| notificationService.ShowConfirmation(parameters.Warning, caption, prompt, parameters.Confirmation);

			return confirmed;
		}

		bool TryAddEventsFromSupporter(IStmALogProvider logParent)
		{
			if (dataStorage.Parent is BusinessObject docDataParent)
			{
				var supporter = docDataParent.GetSupporter();

				if (supporter?.GetMessageLogCreator(document) is IMessageLogCreator logCreator)
				{
					return logCreator.CreateResetToOriginalLog(logParent, document.Data, documentName);
				}
			}

			return false;
		}

		void AddStatusUpdatedEvent(IStmALogProvider logParent)
		{
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					documentName),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
					Enterprise.Core.Constants.EventReferenceMessageTypes.ResetToOriginal),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					Env.CurrentUser.FullName)
			};

			logParent.Logs.CreateOrRecreateEventLog(
				Events.StatusUpdated,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters
			);
		}

		#region Nested Types

		public struct Parameters
		{
			public static Parameters New(MacroMap map)
			{
				var parameters = new Parameters();

				if (map != null)
				{
					parameters.Warning = map.ContainsKey(WarningName)
						? Convert.ToString(map[WarningName], CultureInfo.InvariantCulture)
						: string.Empty;

					parameters.Confirmation = map.ContainsKey(ConfirmationName)
						? Convert.ToString(map[ConfirmationName], CultureInfo.InvariantCulture)
						: string.Empty;
				}

				return parameters;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "MacroMap key constant")]
			public const string WarningName = "Warning";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "MacroMap key constant")]
			public const string ConfirmationName = "Confirmation";

			public string Warning { get; private set; }
			public string Confirmation { get; private set; }
		}

		#endregion
	}
}
