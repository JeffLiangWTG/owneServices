using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class PatternMasterDetail
	{
		public PatternMasterDetail(PatternMasterType masterType, ZGuid masterPk, bool queued)
		{
			MasterType = masterType;
			MasterPk = masterPk;
			Queued = queued;
		}

		public PatternMasterType MasterType { get; }
		public ZGuid MasterPk { get; }
		public bool Queued { get; }

		public override string ToString()
		{
			var pkColumnName = (MasterType is PatternMasterType.OrgHeader) ? OrgHeaderSchema.Constants.PK : GlbPersonSchema.Constants.PK;
			return $"[{pkColumnName}:{MasterPk}]";
		}
	}
}
