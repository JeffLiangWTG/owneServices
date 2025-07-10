using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccountReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccountReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AccountReports; }
		}
	}

	public class CustFilesReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CustFilesReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CustomReports; }
		}
	}

	public class LocationsReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.LocationsReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.LocationReports; }
		}
	}

	public class ProcessMgrReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ProcessMgrReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WorkflowReports; }
		}
	}

	public class BMReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BMReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.BMReports; }
		}
	}

	public class RefFilesReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefFilesReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RefFileReports; }
		}
	}

	public class ArchiveReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ArchiveReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ArchiveReports; }
		}
	}

	public class MasterDataReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.MasterDataReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.MasterDataReports; }
		}
	}

	public class HRReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.HRReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.HRReports; }
		}
	}

	public class SystemReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.SystemReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SystemReports; }
		}
	}

	public class UserAdminReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.UserAdminReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.UserAdminReports; }
		}
	}
}
