using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Startup
{
	class OFXKeyUpgrader : BaseUpgrader
	{
		public OFXKeyUpgrader(IUpgradeManager upgradeManager, DbConnection connection)
			: base(upgradeManager, connection, new VersionLabel(0, 0))
		{ }

		protected override void DoUpgrade()
		{
			StartTask("OFX cryptographic key...");
			using (var command = upgConnection.Command(InsertOrUpdateSql))
			{
				command.ExecuteNonQuery();
			}
		}

		public override int EstimatedNumberOfTasks
		{
			get { return 1; }
		}

		protected override VersionLabel LatestVersion
		{
			get { return new VersionLabel(0, 0); }
		}

		public override string Name
		{
			get { return "OFX Key creation"; }
		}

		public override IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get { return Enumerable.Empty<string>(); }
		}

		const string InsertOrUpdateSql = @"IF NOT EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = 'OFXEncryptionKey')
			BEGIN
				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue) 
						VALUES ('5CDD8B63-CA19-42A0-AB06-1EFFA0BE9731', 'OFXEncryptionKey', NULL, 'STR', CONVERT(VARBINARY(MAX), N'EAAAANOgYRgidl/sx3CjJ/rKE4HYG1dSKXNRBogQkjGzLxpj'))
			END";
	}
}
