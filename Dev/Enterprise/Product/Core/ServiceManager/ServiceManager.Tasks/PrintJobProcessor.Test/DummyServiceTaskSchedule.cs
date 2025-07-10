using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ServiceManager;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing;

public class DummyServiceTaskSchedule : NonPersistentBusinessObject, IServiceTaskSchedule
{
	public DummyServiceTaskSchedule(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	#region Schema
	public static class Schema
	{
		public const string ConfigString = "ConfigString";
		public const string TableName = "ServiceTaskSchedule";
	}
	#endregion

	public string ConfigString { get; set; }

	public string GetBranchCountryCode()
	{
		return "";
	}

	ZGuid IStmScheduleTask.S5_GB { get; set; }
	ZString IStmScheduleTask.S5_ParentTableCode { get; set; }
	ZString IStmScheduleTask.S5_ScheduleDescription { get; set; }
	ZBool IStmScheduleTask.S5_IsActive { get; set; }
	ZString IStmScheduleTask.S5_ScheduleType { get; set; }
	ZDateTime IStmScheduleTask.S5_NextScheduledPrintRunTimeUtc { get; set; }

	public bool IsNudgeable => false;
}
