using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Startup
{
	class KeyCyclingUpgrader : BaseUpgrader
	{
		public KeyCyclingUpgrader(IUpgradeManager upgradeManager, DbConnection connection)
			: base(upgradeManager, connection, new VersionLabel(0, 0))
		{
			keysToCycle = new List<(string, string, uint, bool)>();

			Add("Authentication Encryption Key", "GlowAuthEncryptionKey", keyLengthInBytes: 32u);
			Add("Authentication HMAC Key", "GlowAuthHmacKey", keyLengthInBytes: 64u);
			Add("Login Failure Attempt Secret Key", "LoginFailureAttemptSecretKey", keyLengthInBytes: 64u, forceUpdate: false);
			Add("OIDC Code Verifier Secret Key", "GlowOidcCodeVerifierSecretKey", keyLengthInBytes: 64u, forceUpdate: false);
		}

		readonly List<(string ClientFriendlyName, string Name, uint Length, bool ForceUpdate)> keysToCycle;

		void Add(string clientFriendlyName, string name, uint keyLengthInBytes, bool forceUpdate = true)
		{
			keysToCycle.Add((clientFriendlyName, name, keyLengthInBytes, forceUpdate));
		}

		protected override void DoUpgrade()
		{
			StartTask("Cycling cryptographic keys...");

			foreach (var tuple in keysToCycle)
			{
				StartSubtask(tuple.ClientFriendlyName);

				using (var command = upgConnection.Command(InsertOrUpdateSql))
				{
					command.AddParameter("@RegistryItemName", SqlDbType.VarChar, 8000, tuple.Name);
					command.AddParameter("@NumBytes", SqlDbType.Int, tuple.Length);
					command.AddParameter("@ForceUpdate", SqlDbType.Bit, tuple.ForceUpdate);
					command.AddParameter("@CurrentUser", SqlDbType.VarChar, 3, Db.GetCurrentUserOrDefault(defaultUser: "~BP"));

					command.ExecuteNonQuery();
				}
			}
		}

		public override int EstimatedNumberOfTasks
		{
			get { return 2; }
		}

		protected override VersionLabel LatestVersion
		{
			get { return new VersionLabel(0, 0); }
		}

		public override string Name
		{
			get { return "Key Cycler"; }
		}

		public override IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get { return Enumerable.Empty<string>(); }
		}

		const string InsertOrUpdateSql = @"
DECLARE @randomBytes AS varbinary(max) = CRYPT_GEN_RANDOM(@NumBytes);
IF NOT EXISTS (SELECT null FROM dbo.StmData WHERE SD_Name = @RegistryItemName)
	BEGIN
		INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser)
		VALUES (NEWID(), @RegistryItemName, @randomBytes, GetUtcDate(), @CurrentUser);
	END
ELSE
	BEGIN
		UPDATE dbo.StmData SET
			SD_BinaryValue = @randomBytes,
			SD_SystemLastEditTimeUtc = GetUtcDate(),
			SD_SystemLastEditUser = @CurrentUser
		WHERE 1 = 1
			AND SD_Name = @RegistryItemName
			AND (@ForceUpdate = 1 OR SD_BinaryValue IS NULL OR SD_BinaryValue = 0x);
	END
";
	}
}
