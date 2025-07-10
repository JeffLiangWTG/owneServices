using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Startup
{
	class PTKeyUpgrader : BaseUpgrader
	{
		public PTKeyUpgrader(IUpgradeManager upgradeManager, DbConnection connection)
			: base(upgradeManager, connection, new VersionLabel(0, 0))
		{ }

		protected override void DoUpgrade()
		{
			StartTask("Portugal cryptographic key...");
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
			get { return "PT Key creation"; }
		}

		public override IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get { return Enumerable.Empty<string>(); }
		}

		const string InsertOrUpdateSql = @"IF NOT EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = 'PortugalCertificationKey')
			BEGIN				
				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue) 
						VALUES ('D2593898-557C-457F-B93A-1B86B0D49F7D', 'PortugalCertificationKey', NULL, 'STR', CONVERT(VARBINARY(MAX), N'EAAAAFxFl1cOX5zrNMeXDGdICgFSjkwURs7Fy6aEDwUhHwrAM5PZJdnk31c9fhzWYlqJQxNyJ/bemZPMWK+4MIcs8mc0XnRgDH+PU9FPKwNy8nhBwamd8GcjUx2hxHekDv4Ahz7An+Rc/nfvmuvhTYZtudBIxdSky/3GaFqFrUKdOQWO/CKiapNMr6wyfFjVwenqwrhGpSsfGXwurIYC+jyE6qcJsL4XLhhOIwx7SG0aQxxOmSQSrQRU8863Br4RPg18s9jRj35LRiRfZaQ5qLl1vcDw0MZ8BE63jutvdjzaTG724XVmIN+161pfjwsaux9R/mPaiTjrvZAnSmLJyFzVEqjO34hFEzGTlquXi1UZmilNQMVAjOPzroxISAAqLZR/T2LTsW04diuLCEviXIpRUMCviwWMLE2JhqjbHS7SxwPYJUhw5JrlCCaf/a4vqfLySIIU0MfAxeCd99wsSgukGmo3VJKH/gI8hx0sH6KEH94CHjqvDC9hgZpt+xCRRpTCStn4za2nToHUClQO5P3Ln4yOmp42ceE7KYP+ZwOkWOMX3CVeks56HIxM59HxBLRCgoVddUyEbsgEtz0ZrFCOV3N0pXa238/4JO6feiXHD5ZLa3N/thMOING/rmoKUDtK7szvez9fIq/mml3b7R1H/myAIHAVHl9SwnDWNKTd2ORurK59ccd6XS9rykHzl/5wZEG4UynMhLkzbixV+G9PPcdGcVYCsSb1y0GoUgH6hA3dlMzg57uoDJEaD+kOhKvv9HwkLiNIsU9pf/XcUqneb4zYYEgwkT9tw4jr/CJBAStNPUCPsMCh/pTGbAqXgIfUDpAwI385Zf9GkpbSqIvwCKXiu7/fAG+LPAxsRirqmsdgqVt07JAhJ0soJwQP7+jsqcuEQvob5TWyRzzCLYGLl3HzZ7PiI+GL8pBODRlFcvSRGcMbWOdsqbV7bDVc7S/WhcqwRK2uoRMDEzMz5yxHL+jJ1J7DuDVcQIDt4HpiUKyC02qjAVuZ57l4GHuyp7nDkVLro5mj+NRxUI2LIDgwfUYRoAEQN4UQWvnT7LlHDRaOm1wN6WkCU8BMd2w4IHQwqyEh3m7dbmGHz0BYnFeMojUPqVDLSOI/d7sE9QD8Fkcw5kDFvsvusZlQkZ4IyN1z/NNnq+H2mP8vd3EEOoJy795T7f/04o+FxRcIGUDQ057IIO/woG2OwMIzPiuwpjnjRiJ121cfzJGtp0D30uNlP/WSyjdkDuBYXM68i6EI9XZW'))
			END";
	}
}
