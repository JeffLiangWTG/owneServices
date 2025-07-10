using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	abstract class BusinessIntelligenceUpgradePreparationTest : TestCase
	{
		public abstract void TestCreateDatabaseIfNotExistsMultiUser();

		protected class UpgradeVersionChangeInfoForTest : IVersionChangeInfo
		{
			public UpgradeVersionChangeInfoForTest(VersionLabel version)
			{
				DbReferenceVersion_Schema = version;
			}

			public VersionLabel DbReferenceVersion_Data { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_Schema { get; }
			public VersionLabel DbReferenceVersion_Script { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_CoreScript { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_Transformation { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_Clr { get; } = new VersionLabel(1, 0);

			public bool IsRequired_Data => true;
			public bool IsRequired_Schema => true;
			public bool IsRequired_Script => true;
			public bool IsRequired_Transformation => true;
			public bool IsRequired_ClientDocuments => false;
		}
	}
}
