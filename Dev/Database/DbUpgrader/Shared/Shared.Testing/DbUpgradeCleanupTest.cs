using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	sealed class DbUpgradeCleanupTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileExists()
		{
			// Arrange
			var list = new (string description, string constName, string filePath, string link)[]
			{
				("Offline Mapper", "MapperPath", @"Database\Odyssey\Transformations\Transformations\Transforms\Mapper.cs", "https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2FDbUpgaderCleanUp%2FDbUpgraderCleanUp.cs&version=GBmaster&line=231&lineStyle=plain&lineEnd=232&lineStartColumn=1&lineEndColumn=1"),
				("Upgrade Pre-Schema Change Online Transforms", "SchemaLaunchMapperPath", @"Enterprise\Product\Core\DbUpgrader\Schema\Schema.Launch\Launch\OnlineUpgrade\OnlineMainDatabaseSchemaSynchronisationWrapper.cs", "https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2FDbUpgaderCleanUp%2FDbUpgraderCleanUp.cs&version=GBmaster&line=237&lineStyle=plain&lineEnd=238&lineStartColumn=1&lineEndColumn=1"),
				("System Data Registry", "DataRegistryPath", @"Enterprise\Architecture\Core\Core\Environment\Registry\DataRegistry.cs", "https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2FDbUpgaderCleanUp%2FDbRegistry.cs&version=GBmaster&line=44&lineStyle=plain&lineEnd=45&lineStartColumn=1&lineEndColumn=1"),
				("Build.xml", "buildXmlPath", "build.xml", "https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2FDbUpgaderCleanUp%2FDbOnlineUpgraderCleanUp.cs&version=GBmaster&line=75&lineStyle=plain&lineEnd=76&lineStartColumn=1&lineEndColumn=1"),
			};

			// Act
			var result = list
				.Where(tuple => !File.Exists(Path.Combine(BaseSourcePath, tuple.filePath)))
				.ToList();

			// Assert
			var message = $@"
Constant folder structure used in a satellite project <a href=""https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2F&version=GBmaster"">{CleanupName}</a> has been changed. Please
<ul>
	<li>update listed const value(s) in {CleanupName} to match the current structure</li>
	<li>update mappings in <a href=""https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2FDescription.md&version=GBmaster"">Description.md</a>.</li>
</ul>
<br>
The following elements were updated:
<ul>
{string.Join(string.Empty, result.Select(tuple => $"<li>{tuple.description} <a href=\"{tuple.link}\">{tuple.constName}</a></li>"))}
</ul>";
			HtmlAssertEquals(message, 0, result.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFolderExists()
		{
			var list = new (string description, string constName, string folderPath, string link)[]
			{
				("Upgrade Transforms test projects path", "DataTestPath", @"Database\Odyssey\Transformations\Transformations.Test", "https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2FDbUpgaderCleanUp%2FDbUpgraderCleanUp.cs&version=GBmaster&line=234&lineStyle=plain&lineEnd=235&lineStartColumn=1&lineEndColumn=1"),
			};

			// Act
			var result = list
				.Where(tuple => !Directory.Exists(Path.Combine(BaseSourcePath, tuple.folderPath)))
				.ToList();

			// Assert
			var message = $@"
Constant folder structure used in a satellite project <a href=""https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2F&version=GBmaster"">{CleanupName}</a> has been changed. Please
<ul>
	<li>update listed const value(s) in {CleanupName} to match the current structure</li>
	<li>update mappings in <a href=""https://devops.wisetechglobal.com/wtg/InternalTools/_git/DbUpgraderCleanUp?path=%2FDescription.md&version=GBmaster"">Description.md</a>.</li>
</ul>
<br>
The following elements were updated:
<ul>
{string.Join(string.Empty, result.Select(tuple => $"<li>{tuple.description} <a href=\"{tuple.link}\">{tuple.constName}</a></li>"))}
</ul>";
			HtmlAssertEquals(message, 0, result.Count);
		}

		const string CleanupName = "DbUpgraderCleanup";
	}
}
