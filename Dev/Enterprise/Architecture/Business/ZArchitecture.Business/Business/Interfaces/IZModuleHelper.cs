using System;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public interface IZModuleHelper : IDisposable
	{
		IZLimitedColumnsProvider LimitedColumns { get; set; }
		ModuleIdentifier ParentModuleID { get; set; }
		ModuleIdentifier ID { get; }
		SecurityCheckpoint[] GetSecurityCheckpointForPopups();
		bool SupportsWorkflow { get; }
		bool AllowNew { get; }
	}
}
