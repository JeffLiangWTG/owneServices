using System.IO;

namespace Enterprise.Builder.GenerateDbUpgraderResources.Testing
{
	sealed class SetupForTest : SchemaRegenSetup
	{
		public SetupForTest(string baseDirectory)
			: base()
		{
			this.baseDirectory = baseDirectory;
		}

		protected override void RecreateSchemaFiles(VersionInfoPair versionPair)
		{
			SchemaFileBuilder fileBuilder = new SchemaFileBuilder(Path.Combine(baseDirectory, SourceFileName), MainDbSchemaFilePath, DocManagerDbSchemaFilePath);
			fileBuilder.RecreateFile();
		}

		protected override string VersionFileWindowsPath
		{
			get { return Path.Combine(baseDirectory, VersionFileName); }
		}

		protected override string MainDbSchemaFilePath
		{
			get { return Path.Combine(baseDirectory, MainDbSchemaFileName); }
		}

		protected override string DocManagerDbSchemaFilePath
		{
			get { return Path.Combine(baseDirectory, DocManagerDbSchemaFileName); }
		}

		public const string SourceFileName = "Source.sql";
		public const string ResxFileName = "Resource.resx";
		public const string VersionFileName = "Version.cs";

		public const string MainDbSchemaFileName = "MainDbSchema.sql";
		public const string DocManagerDbSchemaFileName = "DocManagerSchema.sql";

		readonly string baseDirectory;
	}
}
