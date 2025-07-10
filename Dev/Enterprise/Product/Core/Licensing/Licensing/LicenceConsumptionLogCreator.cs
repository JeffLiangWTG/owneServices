using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Licensing
{
	public class LicenceConsumptionLogCreator : ILicenceConsumptionLogCreator
	{
		public void CreateLog(ILicenceCheckpoint licenceCheckpoint)
		{
			CreateLog(licenceCheckpoint, false);
		}

		public void CreateLog(ILicenceCheckpoint licenceCheckpoint, string deviceID, string deviceDetails, int keyStrokes)
		{
			CreateLog(licenceCheckpoint, false, DateTime.MinValue, deviceID, deviceDetails, keyStrokes);
		}

		public void CreateLog(ILicenceCheckpoint licenceCheckpoint, DateTime utcNow)
		{
			CreateLog(licenceCheckpoint, false, utcNow);
		}

		public void CreateLog(ILicenceCheckpoint licenceCheckpoint, bool runInForeground, BusinessObjectFactory factory = null)
		{
			CreateLog(licenceCheckpoint, runInForeground, DateTime.MinValue, factory: factory);
		}

		public void CreateLog(ILicenceCheckpoint licenceCheckpoint, bool runInForeground, DateTime utcNow, string deviceID = "", string deviceDetails = "", int keyStrokes = 0, BusinessObjectFactory factory = null)
		{
			if (!Globals.IsWeb && !runInForeground
#if DEBUG
				&& (!WTG.TestHelpers.TestingState.IsTest || CreateLogOnIdleInTest)
#endif
)
			{
				const int StartDelayMilliseconds = 1000;

				IdleWorker.QueueWorkItemWithOptions(StartDelayMilliseconds, UserIdleWorkItemOptions.DisableSlowRunningWarning,
					new Action(delegate
					{
#if DEBUG
						// This check is needed again since Globals.IsTest can be set after CreateLog was called and before the WorkItem is run
						if (!WTG.TestHelpers.TestingState.IsTest || CreateLogOnIdleInTest)
#endif
						{
							using (PerformanceStatisticsCollector.StartMonitoring("Consume licence"))
							{
								CreateLogDirectly(licenceCheckpoint, utcNow, deviceID, deviceDetails, keyStrokes, factory);
							}
						}
					}), null);
			}
			else
			{
				using (PerformanceStatisticsCollector.StartMonitoring("Consume licence"))
				{
					CreateLogDirectly(licenceCheckpoint, utcNow, deviceID, deviceDetails, keyStrokes, factory);
				}
			}
		}

#if DEBUG
		protected bool CreateLogOnIdleInTest;
#endif

		void CreateLogDirectly(ILicenceCheckpoint licenceCheckpoint, DateTime utcNow, string deviceID, string deviceDetails, int keyStrokes, BusinessObjectFactory factory = null)
		{
			// A normal windows login will always have a branch and user.
			// A web login will have user ZZ. Branch will be initially from the web.config. After an OrgContact logs in, the branch will be calculated from the Org.
			// A user context switch should always supply a branch and user.
			// However it is possible to set the current user context to an empty value.
			// In this case, the current licences will have no user and branch.
			var parentUserContext = licenceCheckpoint.ParentUserContext;
			if (parentUserContext != null &&
				parentUserContext.BranchPk != Guid.Empty &&
				!string.IsNullOrEmpty(parentUserContext.UserInitials))
			{
				if (Db.Connection.IsInTransaction && factory == null
#if DEBUG
					&& !WTG.TestHelpers.TestingState.IsTest
#endif
					)
				{
					using (var connection = Db.NewExtraConnectionToMainDb())
					{
						CreateLogAndSave(licenceCheckpoint, utcNow, deviceID, deviceDetails, keyStrokes, new BusinessObjectFactory(connection) { RefreshEnabled = false }, true);
					}
				}
				else
				{
					CreateLogAndSave(licenceCheckpoint, utcNow, deviceID, deviceDetails, keyStrokes, factory ?? new BusinessObjectFactory() { RefreshEnabled = false }, factory == null);
				}
			}
		}

		void CreateLogAndSave(ILicenceCheckpoint licenceCheckpoint, DateTime utcNow, string deviceID, string deviceDetails, int keyStrokes, BusinessObjectFactory factory, bool needFactorySave)
		{
			try
			{
				CreateLogInternal(licenceCheckpoint, utcNow, deviceID, deviceDetails, keyStrokes, factory);
				if (needFactorySave)
				{
					factory.Save();
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
		}

		protected virtual StmActivityLog CreateLogInternal(ILicenceCheckpoint checkpoint, DateTime utcNow, string deviceID, string deviceDetails, int keyStrokes, BusinessObjectFactory factory)
		{
			StmActivityLog log = factory.New<StmActivityLog>();
			log.S7_EnterpriseActivity = true;
			log.S7_FormCaption = checkpoint.Name;
			log.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			if (Globals.IsWeb)
			{
				var webControllerID = $"{log.S7_ControllerID}{GetWebUserEmailAndName()}";
				if (webControllerID.Length > StmActivityLogSchema.S7_ControllerID.MaxLength)
				{
					webControllerID = webControllerID.Substring(0, StmActivityLogSchema.S7_ControllerID.MaxLength);
				}
				log.S7_ControllerID = webControllerID;
			}
			log.S7_OpenDateTimeUtc = utcNow != DateTime.MinValue ? utcNow : ZDateTime.UtcNow;
			log.S7_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			var parentUserContext = checkpoint.ParentUserContext;
			log.S7_ParentID = parentUserContext.BranchPk;
			log.S7_DeviceID = string.IsNullOrWhiteSpace(deviceID) ? string.Empty : deviceID;
			log.S7_DeviceDetails = string.IsNullOrWhiteSpace(deviceDetails) ? null : deviceDetails;
			log.S7_GS_NKUser = parentUserContext.UserInitials;
			log.S7_MouseClicks = (int)checkpoint.LicenceType;
			log.S7_KeyStrokes = keyStrokes;

			return log;
		}

		public const string LicensingHiddenNoteDescription = "Licensing Tracking Note";

		protected virtual string GetWebUserEmailAndName()
		{
			var webEnv = Env.Instance as IWebEnvironment;
			var webUser = webEnv?.WebUser;

			return webUser != null ? $"|{webUser.Email}|{webUser.Name}" : string.Empty;
		}

		#region Implementation

		IIdleWorker IdleWorker
		{
			get { return ObjectFactory.Get<IIdleWorker>(); }
		}

		#endregion
	}
}
