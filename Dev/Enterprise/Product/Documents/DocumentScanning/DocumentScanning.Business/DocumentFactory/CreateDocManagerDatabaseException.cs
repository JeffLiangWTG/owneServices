using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class CreateDocManagerDatabaseException : OdysseyException, IExceptionReporterExtender
	{
		public CreateDocManagerDatabaseException(string errorMessage, Exception innerException)
			: base(errorMessage, innerException)
		{
		}

		public CreateDocManagerDatabaseException(string errorMessage)
			: base(errorMessage)
		{
		}

#if NETFRAMEWORK
		protected CreateDocManagerDatabaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region IExceptionReporterExtender Members

		bool IExceptionReporterExtender.HandleException()
		{
			try
			{
				using (FactorySaveAlerterOverride.TemporarilyOverride(this))
				{
					EnvProxy.Instance.OutgoingMailManager.CreateAndSaveToCompanyNotificationGroup(Res.GetString("44b9deef-3589-4482-96fc-1570ba9659cf", "Error creating database"), Message);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				// Already handling a previous exception.
				// If fails to send the email, no point on throwing another exception.
				// A pop up window will be displayed anyway.
			}

			Globals.Message.ShowError((NoResString)"Error creating database.\r\n\r\nReason : " + Message + PostMessage);// this is called from code that used to do this anyway. BS 14/04/09
			return true;
		}

		protected virtual string PostMessage => "\r\n\r\n" + Res.GetString("3025f1de-79e7-4e36-887d-70055a8af8d7", "To fix this, please ask your system administrator to go into the system registry and update the paths for '{0}' and '{1}'. These need to be paths that the SQL Server can access from the machine that is running the SQL Server service itself. The path is relative to the machine that is running the SQL Server service.",
																			((IRegistryItemInternals)SystemDataRegistry.Instance.DocManagerDBDataFilePath).Location,
																			((IRegistryItemInternals)SystemDataRegistry.Instance.DocManagerDBLogFilePath).Location);

		#endregion
	}

	[Serializable]
	public class CanNotAcquireLockForCreatingSDDatabaseException : CreateDocManagerDatabaseException
	{
		public CanNotAcquireLockForCreatingSDDatabaseException(string errorMessage) : base(errorMessage)
		{
		}

#if NETFRAMEWORK
		protected CanNotAcquireLockForCreatingSDDatabaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		protected override string PostMessage => "\r\n\r\n" + Res.GetString("43EDCDD4-1B4D-4FA2-89B3-9A659BF79BF7", "If you continually see this message, please contact your administrator or raise an incident.");
	}
}
