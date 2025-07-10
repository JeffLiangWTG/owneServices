using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CA.Business.OperationalAction
{
	public class CAB3OperationalActionRunner
	{
		public CAB3OperationalActionRunner(OperationalActionLogAndUserNotificationWrapper logandNotificationWrapper, ISendsMessagesToCustoms sendMessagesToCustoms, ZBool isNeedDoMerge)
		{
			this.logAndNotificationWrapper = logandNotificationWrapper;
			this.sendMessagesToCustoms = sendMessagesToCustoms;
			this.isNeedDoMerge = isNeedDoMerge;
		}

		readonly OperationalActionLogAndUserNotificationWrapper logAndNotificationWrapper;
		readonly ISendsMessagesToCustoms sendMessagesToCustoms;
		readonly ZBool isNeedDoMerge;

		public void PerformFunctionOperationalAction(BusinessObject[] targets)
		{
			var result = new List<ZGuid>();
			if (targets.Length == 0)
			{
				logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Error, Constants.NoDeclarationToSend);
			}
			else
			{
				logAndNotificationWrapper.SetSectionProgressMax(targets.Length);
				RunOperationalActionSendMessage(targets);
			}
		}

		void RunOperationalActionSendMessage(BusinessObject[] targets)
		{
			foreach (var target in targets)
			{
				logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Informational, Constants.Seperator);
				var newFactory = new BusinessObjectFactory();
				var targetDeclaration = newFactory.Load<JobDeclaration>(target.PK);

				if (this.isNeedDoMerge)
				{
					var messageCollector = new SendsMessagesToCustomsShutterUpperer(false);
					if (targetDeclaration.DoMerge(messageCollector))
					{
						logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Constants.SuccessfullyMergedDeclaration + Constants.NotifyFormatParaHolder, targetDeclaration.GetDeclarationIdLink());
						try
						{
							newFactory.Save();
						}
						catch (ZSaveException e)
						{
							ZExceptionReporting.HandleSaveException(e);
						}
					}
					else
					{
						logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Warning, Constants.CannotMergeDeclaration + Constants.NotifyFormatParaHolder + Constants.MergeFailedReason + messageCollector.InvalidOperationText, targetDeclaration.GetDeclarationIdLink());
					}
				}

				var errorMessage = string.Empty;
				if (IsJobEligibleForSending(targetDeclaration, out errorMessage))
				{
					logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Constants.SendingMessageForDeclaration + Constants.NotifyFormatParaHolder, targetDeclaration.GetDeclarationIdLink());
					logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Informational, SendMessageForDeclaration(targetDeclaration) ? Constants.Successful : Constants.Failed);
				}
				else if (!string.IsNullOrWhiteSpace(errorMessage))
				{
					logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Error, errorMessage);
				}
			}
		}

		bool IsJobEligibleForSending(JobDeclaration targetDeclaration, out string errorMessage)
		{
			var result = false;
			if (targetDeclaration == null)
			{
				errorMessage = Constants.InvalidDeclaration;
			}
			else
			{
				logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Constants.ProcessingDeclaration + Constants.NotifyFormatParaHolder, targetDeclaration.GetDeclarationIdLink());
				if (!(targetDeclaration.IsImport))
				{
					errorMessage = Constants.DeclarationNotEligable;
				}
				else if (targetDeclaration.ShowSubmitMenuItem)
				{
					errorMessage = Constants.DeclarationSubmitThroughInterface;
				}
				else
				{
					var checker = new CanSendDeclarationChecker(targetDeclaration);
					try
					{
						result = checker.AtLeastOneEntryExists() && (checker.RequiredEntryExist(MessageTypeList.Codes.B3CUSDEC) || checker.RequiredEntryExist(MessageTypeList.Codes.CommercialAccountingDeclaration));
						errorMessage = checker.LastErrorMessage;
					}
					catch (System.ArgumentException ex)
					{
						result = false;
						errorMessage = ex.Message;
					}
				}
			}
			return result;
		}

		bool SendMessageForDeclaration(JobDeclaration targetDeclaration)
		{
			var result = false;

			targetDeclaration.MessageInitiator = sendMessagesToCustoms;
			var entryHeader = targetDeclaration.B3EntryHeader;
			if (entryHeader.IsB3C)
			{
				var dataWrapper = targetDeclaration.IsLVS ? (IB3Header)new LowValueShipmentsMessageWrapper(entryHeader) : new B3ImportMessageWrapper(entryHeader);
				result = new B3ImportMessageManager(dataWrapper, logAndNotificationWrapper).SendMessage(MessageSubTypes.Create);
			}
			else if (entryHeader.IsCAD)
			{
				var wrapper = new CADMessageWrapper(entryHeader);
				result = new CADMessageManager(wrapper, logAndNotificationWrapper).SendMessage();
			}
			return result;
		}
	}
}
