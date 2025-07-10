using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Data
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class will be implemented in the future.")]
	public class DBProcessLock : IDisposable
	{
		public DBProcessLock(ProcessTypeEnum processType, string processIdentifier)
		{
			fProcessType = processType;
			fProcessIdentifier = processIdentifier;
		}

		public DBProcessLock(ProcessTypeEnum processType)
			: this(processType, "")
		{ }

		#region Lock and Unlock

		public bool Lock()
		{
			IsUnlockPending = true;
			return true;
		}

		public bool Unlock()
		{
			IsUnlockPending = false;
			return true;
		}

		public bool IsLockedByThisInstance()
		{
			return IsUnlockPending;
		}

		protected bool IsUnlockPending;

		#endregion

		public enum ProcessTypeEnum
		{
			None = 0,
			ClientSpecificBatchProcessor = 13,
			ClientSpecificBatchProcessorWithClientSpecificMode = 14,
			ZACInbound = 21,
			ZACOutbound = 22,
			AECustomsInbound = 27,
			AECustomsOutbound = 28,
			ShipnetOutbound = 33,
			USCustomsInbound = 34,
			USCustomsOutbound = 35,
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Batch Processor Name")]
		public static string GetProcessName(ProcessTypeEnum process)
		{
			switch (process)
			{
				case ProcessTypeEnum.None:
					return "";
				case ProcessTypeEnum.ClientSpecificBatchProcessor:
					return "Client Specific Batch Processor";
				case ProcessTypeEnum.ClientSpecificBatchProcessorWithClientSpecificMode:
					return "Client Specific Batch Processor - specific mode";
				case ProcessTypeEnum.ZACInbound:
					return "South African Customs - Inbound";
				case ProcessTypeEnum.ZACOutbound:
					return "South African Customs - Outbound";
				case ProcessTypeEnum.AECustomsInbound:
					return "UAE - Inbound";
				case ProcessTypeEnum.AECustomsOutbound:
					return "UAE - Outbound";
				case ProcessTypeEnum.ShipnetOutbound:
					return "Shipnet - Outbound";
				case ProcessTypeEnum.USCustomsInbound:
					return "US Customs - Inbound";
				case ProcessTypeEnum.USCustomsOutbound:
					return "US Customs - Outbound";

				default:
					return ProcessNameUnknown;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Batch Processor Name")]
		public const string ProcessNameUnknown = "Unknown";

		public class LockInfo
		{
			public LockInfo()
			{
				ProcessType = ProcessTypeEnum.None;
			}

			public int SqlProcId;
			public DateTime LockTime;
			public ProcessTypeEnum ProcessType;
			public string ProcessName = "";
			public string ProcessIdentifier = "";
			public string ClientPCName = "";
			public string ClientPCIP = "";
			public string ClientOSUser = "";
			public string CompanyName = "";
			public string BranchName = "";
			public string Comments = "";
		}

		public LockInfo GetRunningProcessInfo()
		{
			LockInfo result = null;

			if (IsUnlockPending)
			{
				result = new LockInfo();

				result.SqlProcId = Db.Connection.SPID;
				result.ClientPCName = System.Environment.MachineName;
				result.ClientOSUser = System.Environment.UserName;
				result.ClientPCIP = Utilities.GetLocalIPAddress();
				result.ProcessType = fProcessType;
				result.ProcessIdentifier = fProcessIdentifier;
				result.ProcessName = GetProcessName(fProcessType);
				result.CompanyName = ((Environment.EnvProxy.Instance.CurrentCompany == null)) ? "" : Environment.EnvProxy.Instance.CurrentCompany.Name;
				result.BranchName = ((Environment.EnvProxy.Instance.CurrentBranch == null)) ? "" : Environment.EnvProxy.Instance.CurrentBranch.Name;
				result.Comments = "";
				//Result.LockTime = must store when locking...;
			}

			return result;
		}

		public void Dispose()
		{
			IsUnlockPending = false;
		}

		public static LockInfo[] GetAllRunningProcessInfo()
		{
			return Array.Empty<LockInfo>();
		}

		public ProcessTypeEnum ProcessType
		{
			get { return fProcessType; }
		}

		public string ProcessIdentifier
		{
			get { return fProcessIdentifier; }
		}

		protected ProcessTypeEnum fProcessType;
		protected string fProcessIdentifier;
	}
}
