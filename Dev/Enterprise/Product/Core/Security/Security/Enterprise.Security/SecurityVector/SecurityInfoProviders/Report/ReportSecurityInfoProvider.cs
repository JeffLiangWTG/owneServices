using System.Collections.Generic;

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	class ReportSecurityInfoProvider : SecurityInfoProvider
	{
		public ReportSecurityInfoProvider(SecurityInfoProvider parent, StmMenuItem item)
			: base(parent, GetReportCheckPoint(parent, item))
		{ }

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			yield break;
		}

		public override string Name { get { return Checkpoint.DisplayText; } }

		static SecurityCheckpoint GetReportCheckPoint(SecurityInfoProvider parent, StmMenuItem item)
		{
			if (parent.Security.FindCheckPoint(new CheckpointLookupKey((NoResString)"Report", item.PK.ToGuid())) == null)
			{
				return new SecurityCheckpoint("Report", item.SU_MenuNameMultilingual, (SecurityCheckpoint)parent.Checkpoint, parent.Security, item.PK.ToGuid());
			}
			else
			{
				return null;
			}
		}
	}
}
