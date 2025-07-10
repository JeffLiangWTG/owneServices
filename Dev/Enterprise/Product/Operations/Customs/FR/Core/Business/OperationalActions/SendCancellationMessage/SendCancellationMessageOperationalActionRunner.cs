using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class SendCancellationMessageOperationalActionRunner
	{
		public SendCancellationMessageOperationalActionRunner(IOperationalActionSectionLog log, BusinessObjectFactory factory)
		{
			logger = new OperationalActionSectionLogWrapper(log);
			this.factory = factory;
		}
		readonly ICommonLogger logger;
		readonly BusinessObjectFactory factory;

		public void SendCancellationMessages(SendCancellationMessageDataObject dataObject)
		{
			if (dataObject.HasErrors)
			{
				dataObject.Notifications.GetErrors().Select(x => x.Message).ForEach(message => logger.LogFormat(LogType.Error, (NoResString)message));
			}
			else
			{
				var errorCollector = new ErrorCollector();
				var entries = dataObject.Targets.OfType<JobDeclaration>()
							.SelectMany(s => s.ActiveEntryHeaders)
							.Cast<CusEntryHeader>()
							.Concat(dataObject.Targets.OfType<CusEntryHeader>());

				var messageBatchNumberPopulator = new MessageBatchNumberPopulator(factory);

				var entriesCount = entries.Count();
				var listOfMessageSender = new Dictionary<DeltaIEMessageSender, string>();
				foreach (var entry in entries)
				{
					var sendingObject = new DeltaIEJobDeclarationMessageSendingObject(entry);
					sendingObject.ChangeAcknowledgementIndicator = dataObject.InvalidationMotivation.Left(sendingObject.ChangeAcknowledgementIndicatorInfo.MaxLength);
					sendingObject.VOCReason = dataObject.InvalidationReason.Left(sendingObject.VOCReasonInfo.MaxLength);
					sendingObject.MessageType = DeltaIESendMessageSubTypeList.Codes.Invalidation;
					sendingObject.WrapperModifier = DeltaIEJobDeclarationMessageSendingObject.Schema.ForOperationalAction;
					sendingObject.ShouldSend = true;
					var messageSender = new DeltaIEMessageSender(sendingObject, errorCollector);

					if (entriesCount > 1)
					{
						messageSender.MessageDecoratorForSaving = x => messageBatchNumberPopulator.PopulateBatchNumber((FREDIMessage)x);
						messageSender.Send(true);
					}
					else
					{
						messageSender.Send();
						logger.LogFormat(LogType.Information, (NoResString)"Cancellation message for entry {0} queued for sending successfully", entry.CH_BGMReference);
					}
				}
				var result = ZString.Empty;

				if (entriesCount > 1)
				{
					try
					{
						var entryForSaving = entries.FirstOrDefault();
						entryForSaving.Factory.Save();
						foreach (var entry in entries)
						{
							logger.LogFormat(LogType.Information, (NoResString)"Cancellation message for entry {0} queued for sending successfully", entry.CH_BGMReference);
						}
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
						result = FormattableString.Invariant($"{DeltaIEMessageSender.MessageCreateFailure}\n{ex.Message}");
						errorCollector.AddError(result);
					}
					catch (Exception ex) when (!CargoWise.Common.ExceptionExtensions.IsCriticalException(ex))
					{
						result = FormattableString.Invariant($"{DeltaIEMessageSender.MessageSendFailure}\n{ex.Message}");
						errorCollector.AddError(result);
					}
				}
			}
		}
	}
}
