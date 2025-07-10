using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class AutoSendNCTSMessageProcessor : IProcessor
	{
		public AutoSendNCTSMessageProcessor(NctsHeader nctsHeader)
		{
			Header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}
		protected readonly NctsHeader Header;

		protected abstract ZString MessageDescription { get; }

		protected abstract ZBool SendNctsMessageCore(INotifications notifications);

		protected void LogSystemError(INotifications notifications, ZString errorMessage)
			=> notifications.AddError(ZString.Format((NoResString)"There was a system error while attempting sending {0} message. See below for more information.\r\n{1}\r\n", MessageDescription, errorMessage));

		protected virtual ZBool CanSendNctsHeader() => true;

		protected virtual void ValidateHeaderCore() => Header.RunPreSaveValidation();

		ZBool ValidateHeaderBeforeSendingMessage(INotifications notifications)
		{
			ValidateHeaderCore();
			if (Header.HasErrors)
			{
				notifications.AddError(LogErrorsOnHeader);
				return true;
			}
			return false;
		}

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (Header.HasChanges)
			{
				if (Header.HasErrors)
				{
					notifications.AddError(LogErrorsOnHeader);
					return;
				}

				try
				{
					Header.Factory.Save();
				}
				catch (ZSaveException e)
				{
					LogSystemError(notifications, e.Message);
					return;
				}
			}

			var hasMessageBeenGenerated = false;
			var nctsHeaderMutex = GetNctsHeaderMutexToSendMessage();
			try
			{
				if (nctsHeaderMutex.Lock())
				{
					if (CanSendNctsHeader())
					{
						var hasErrorsOnHeader = ValidateHeaderBeforeSendingMessage(notifications);
						if (!hasErrorsOnHeader)
						{
							hasMessageBeenGenerated |= SendNctsMessageCore(notifications);
						}
					}
					else if (Header.Messages.IsWaitingForAResponse)
					{
						notifications.AddWarning(ZString.Format((NoResString)"System cannot send the {0} message for Job:{1}, please check whether {0} is waiting for response from customs.", MessageDescription, Header.BH_JobReference));
					}
					else
					{
						notifications.AddWarning(ZString.Format((NoResString)"System cannot send the {0} message for Job:{1}.", MessageDescription, Header.BH_JobReference));
					}
				}
				else
				{
					var mutexLockedByInfo = nctsHeaderMutex.GetMutexLockByInfo();
					notifications.AddError(ZString.Format((NoResString)"System cannot send the {0} message for Job:{1}, as {2} is trying to send the same message for this entry. Please wait until the lock has been released before trying to send the message again.", MessageDescription, Header.BH_JobReference, mutexLockedByInfo));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				LogSystemError(notifications, e.Message);
			}
			finally
			{
				if (nctsHeaderMutex != null && nctsHeaderMutex.HasLock)
				{
					nctsHeaderMutex.Unlock();
				}
			}

			if (hasMessageBeenGenerated)
			{
				try
				{
					Header.Factory.Save();
					notifications.Add(CargoWise.ComponentModel.NotificationType.Information, ZString.Format((NoResString)"{0} message has been sent to customs for Job:{1}", MessageDescription, Header.BH_JobReference));
				}
				catch (ZSaveException e)
				{
					LogSystemError(notifications, e.Message);
				}
			}
		}

		ZString LogErrorsOnHeader => ZString.Format((NoResString)"System cannot send {0} message because of following errors on Job:{1}, please fix all of them and try again.\r\n{2}", MessageDescription, Header.BH_JobReference, Header.GetErrors().ToUniqueMessageListString());

		ZGlobalMutex GetNctsHeaderMutexToSendMessage()
			=> nctsHeaderMutexToSendMessage ??= new ZGlobalMutex(ZArchitecture.Modules.MutexIDs.SendCustomsMessage, Header.PK.ToString());

		ZGlobalMutex nctsHeaderMutexToSendMessage;
	}
}
