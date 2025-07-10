using System;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class VersionChangeInfoForTest : IVersionChangeInfo
	{
		public VersionLabel DbReferenceVersion_Data => throw new NotImplementedException();
		public VersionLabel DbReferenceVersion_Schema { get; } = new VersionLabel(1502, 6);
		public VersionLabel DbReferenceVersion_Script { get; } = new VersionLabel(0, 0);
		public VersionLabel DbReferenceVersion_CoreScript { get; } = new VersionLabel(0, 0);
		public VersionLabel DbReferenceVersion_Transformation { get; } = new VersionLabel(0, 0);
		public VersionLabel DbReferenceVersion_Clr { get; set; } = new VersionLabel(1, 0);

		public bool IsRequired_ClientDocuments => false;
		public bool IsRequired_Data => false;
		public bool IsRequired_Schema => false;

		public bool OverrideIsRequired_Script { get; set; }
		public bool IsRequired_Script => OverrideIsRequired_Script;

		public bool OverrideIsRequired_Transformation { get; set; }
		public bool IsRequired_Transformation => OverrideIsRequired_Transformation;
		public bool WithPreUpgrade => false;
		public bool IsRequired_Clr => false;
	}
}
