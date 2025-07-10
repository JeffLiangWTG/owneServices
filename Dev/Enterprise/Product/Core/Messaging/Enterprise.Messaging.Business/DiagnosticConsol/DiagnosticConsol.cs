using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.Business
{
	public abstract class DiagnosticConsol : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected DiagnosticConsol(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString ConsolKey { get; private set; }
		public ZGlobalMutex Mutex1 { get; private set; }
		protected ZGuid messagePK;
		protected DiagnosticNote diagnosticNote;

		public virtual ZString InitialMessage
		{
			get { return ZString.Empty; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an Identifier")]
		public List<ZString> AllocateKeyAndDoInitialChecks()
		{
			List<ZString> result = new List<ZString>();
			ConsolKey = ZGuid.NewZGuid().ToString();
			MutexID mutexID = new MutexID(ConsolKey, "Lock each instance of a Diagnostic Consol");
			Mutex1 = new ZGlobalMutex(mutexID);
			if (Mutex1.Lock())
			{
				try
				{
					if (IsTestMode)
					{
						result.Add(Res.GetString("6c4cd6a6-0194-4cb2-9b0f-da06962c8810", "NOTE: You are running in 'Test Mode'; i.e. not in production mode yet."));
					}

					messagePK = CreateTestMessage(Factory, ConsolKey);
					if (messagePK.IsEmpty)
					{
						throw new ApplicationException("Failed to create test message.");
					}

					CreateDiagStmNoteData();
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Mutex1.Unlock();
					throw;
				}
			}
			else
			{
				throw new ApplicationException(string.Format("Failed to lock Mutex: {0}.", ConsolKey));
			}
			return result;
		}

		protected virtual void CreateDiagStmNoteData()
		{
			diagnosticNote = new DiagnosticNote(ConsolKey, InitialStatus, DiagnosticConsolActions.Codes.LogMessage, messagePK);
		}

		public void ReleaseDiagnosticLock()
		{
			ReleaseResources();
			if (Mutex1 != null && Mutex1.IsLocked)
			{
				Mutex1.Unlock();
			}
		}

		protected virtual void ReleaseResources()
		{
		}

		protected virtual bool IsTestMode
		{
			get { return false; }
		}

		protected virtual ZString WaitingMessage
		{
			get { return Res.GetString("3081a5e4-5d4a-4930-8127-3928da5f67bc", "Test message created.\r\nWaiting for test message status to change from 'QUE'..."); }
		}

		protected virtual ZString InitialStatus
		{
			get { return DiagnosticStatusList.Codes.TestMessageAtQUEStatus; }
		}

		public ZString GetExtendedStatusDescriptionForCurrentStatus()
		{
			ZString result;
			var currentStatus = GetCurrentDiagnosticStatus();
			result =
				"===============\r\n" +
				Res.GetString("c0ebbd3a-128b-4458-a88e-8066c3993b89", "Current status is {0} : {1}", currentStatus, new DiagnosticStatusList().GetDescriptionFromCode(currentStatus)) +
				"\r\n" + GetExtendedStatusDescription(currentStatus) + "\r\n===============";
			return result;
		}

		protected virtual ZString GetCurrentDiagnosticStatus()
		{
			return diagnosticNote.Status;
		}

		protected virtual ZString GetExtendedStatusDescription(ZString currentStatus)
		{
			return DiagnosticStatusList.GetExtendedStatusDescription(currentStatus);
		}

		public override string TableName
		{
			get
			{
				return "DiagnosticConsol[NonPersistent]";
			}
		}

		public abstract ZString ProcessTimerTick(out ZString diagnosticConsolAction);
		protected abstract ZGuid CreateTestMessage(BusinessObjectFactory testMessageFactory, ZString consolKey);
		public abstract List<RegistryAndCertificateCheckResult> RegistryAndCertificateChecks();

		public (List<string> Messages, bool IsAllHealthyOrWarning) ProcessRegistryAndCertificateCheckResults()
		{
			var messages = new List<string>();
			bool isAllHealthyOrWarning = true;

			foreach (var checkResult in RegistryAndCertificateChecks())
			{
				messages.Add(checkResult.Message);

				if (checkResult.Level != StabilityResultLevel.Healthy && checkResult.Level != StabilityResultLevel.Warning)
				{
					isAllHealthyOrWarning = false;
				}
			}

			return (messages, isAllHealthyOrWarning);
		}
	}
}
