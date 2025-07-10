using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class EInvoicingEmailNotificationCreator
	{
		protected EInvoicingEmailNotificationCreator(ILogger logger)
		{
			ServiceLogger = logger;
		}

		readonly protected ILogger ServiceLogger;

		public abstract void SendEmail();

		protected void LogDefaultUnsuccessfulEmailSentResult(EInvoicingTransactionErrorNotificationEmail email, ZString companyName)
		{
			var errorMessages = new ZStringBuilder(email.GetAllErrorsThatOccuredWhileSendingEmail());
			ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Email Notification was not sent for {0}. {1}", companyName, errorMessages.ToStringWithNewLineBetweenAppends()));
		}

		protected DisposableAction SwitchCompanyContextTemporarilyIfRequires(ZGuid branchPK)
		{
			IDisposable tempContext = null;

			Action createAction = () =>
			{
				if (branchPK.ToGuid() != Env.CurrentBranchPK)
				{
					tempContext = Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchPK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
				}
			};

			Action disposeAction = () => tempContext?.Dispose();

			return new DisposableAction(createAction, disposeAction);
		}
	}
}