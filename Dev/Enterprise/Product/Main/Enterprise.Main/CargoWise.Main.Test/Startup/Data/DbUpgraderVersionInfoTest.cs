using System;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Startup.Testing
{
	sealed class DbUpgraderVersionInfoTest : TransactionedTestCase
	{
		public void TestAllVersionsAlreadySynchronisedWhenRunningTests()
		{
			IVersionChangeInfo versionInfo = new DbUpgraderVersionInfo();

			AssertEquals("Data Version Synchronised", 0, DataVersion.Application.CompareTo(versionInfo.DbReferenceVersion_Data));
			AssertEquals("Schema Version Synchronised", 0, SchemaVersion.Application.CompareTo(versionInfo.DbReferenceVersion_Schema));
			AssertEquals("Script Version Synchronised", 0, ScriptVersion.Application.CompareTo(versionInfo.DbReferenceVersion_Script));
			AssertEquals("Transformation Version Synchronised", 0, TransformationVersion.ApplicationNumber.CompareTo(versionInfo.DbReferenceVersion_Transformation));

			AssertEquals("Data Upgrade Required", false, versionInfo.IsRequired_Data);
			AssertEquals("Schema Upgrade Required", false, versionInfo.IsRequired_Schema);
			AssertEquals("Script Upgrade Required", false, versionInfo.IsRequired_Script);
			AssertEquals("Post Upgrade Required", false, versionInfo.IsRequired_Transformation);
		}

		public void TestSchemaVersion()
		{
			DbUpgraderVersionInfoForTesting versionInfo = new DbUpgraderVersionInfoForTesting();
			var iVersionInfo = versionInfo as IVersionChangeInfo;

			// App = DB [PRE-CONDITION]
			//versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major, SchemaVersion.Application.Minor);
			AssertEquals("[PRE-CONDITION] Schema Version (App = DB)", 0, SchemaVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Schema));
			AssertEquals("[PRE-CONDITION] Schema Upgrade Required (App = DB)", false, iVersionInfo.IsRequired_Schema);

			// App < DB and !Major
			versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major, SchemaVersion.Application.Minor + 1);
			AssertEquals("Schema Version (App < DB and !Major)", true, SchemaVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Schema) < 0);
			AssertEquals("Schema Upgrade Required (App < DB and !Major)", true, iVersionInfo.IsRequired_Schema);

			// App < DB and Major
			versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major + 1, SchemaVersion.Application.Minor);
			AssertEquals("Schema Version (App < DB and Major)", true, SchemaVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Schema) < 0);
			AssertEquals("Schema Upgrade Required (App < DB and Major)", false, iVersionInfo.IsRequired_Schema);

			// App > DB and !Major
			versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major, SchemaVersion.Application.Minor - 1);
			AssertEquals("Schema Version (App > DB and !Major)", true, SchemaVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Schema) > 0);
			AssertEquals("Schema Upgrade Required (App > DB and !Major)", true, iVersionInfo.IsRequired_Schema);

			// App > DB and Major
			versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major - 1, SchemaVersion.Application.Minor);
			AssertEquals("Schema Version (App > DB and Major)", true, SchemaVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Schema) > 0);
			AssertEquals("Schema Upgrade Required (App > DB and Major)", true, iVersionInfo.IsRequired_Schema);
		}

		public void TestScriptVersion()
		{
			DbUpgraderVersionInfoForTesting versionInfo = new DbUpgraderVersionInfoForTesting();
			IVersionChangeInfo iVersionInfo = versionInfo;

			// App = DB [PRE-CONDITION]
			AssertEquals("[PRE-CONDITION] Core Script Version (App = DB)", 0, ScriptVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Script));
			AssertEquals("[PRE-CONDITION] Core Script Upgrade Required (App = DB)", false, iVersionInfo.IsRequired_Script);

			// App < DB
			versionInfo.ReInitialiseDbReferenceVersion_Script(ScriptVersion.Application.Major + 1, ScriptVersion.Application.Minor);
			AssertEquals("Core Script Version (App < DB)", true, ScriptVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Script) < 0);
			AssertEquals("Core Script Upgrade Required (App < DB)", true, iVersionInfo.IsRequired_Script);

			// App > DB
			versionInfo.ReInitialiseDbReferenceVersion_Script(ScriptVersion.Application.Major - 1, ScriptVersion.Application.Minor);
			AssertEquals("Core Script Version (App > DB)", true, ScriptVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Script) > 0);
			AssertEquals("Core Script Upgrade Required (App > DB)", true, iVersionInfo.IsRequired_Script);
		}

		/// <summary>
		/// SCHEMA BOUND VIEWS must be removed before upgrading the database schema.
		/// To ensure they're recreated, the SCRIPT UPBRADE must always run after a SCHEMA UPGRADE.
		/// </summary>
		public void TestScriptVersionIsRequiredWhenSchemaVersionIsRequired()
		{
			DbUpgraderVersionInfoForTesting versionInfo = new DbUpgraderVersionInfoForTesting();
			var iVersionInfo = versionInfo as IVersionChangeInfo;

			// Script Version App = DB [PRE-CONDITION]
			AssertEquals("[PRE-CONDITION] Script Version (App = DB)", 0, ScriptVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Script));
			AssertEquals("[PRE-CONDITION] Script Upgrade Required (App = DB)?", false, iVersionInfo.IsRequired_Script);

			// SCHEMA VERSION: App < DB and !Major
			versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major, SchemaVersion.Application.Minor + 1);
			AssertEquals("Schema Upgrade Required (App < DB and !Major)?", true, iVersionInfo.IsRequired_Schema);
			AssertEquals("Script Upgrade Required (no script version diff, but schema upgrade is required)?", true, iVersionInfo.IsRequired_Script);

			// SCHEMA VERSION: App < DB and Major
			versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major + 1, SchemaVersion.Application.Minor);
			AssertEquals("Schema Upgrade Required (App < DB and Major)?", false, iVersionInfo.IsRequired_Schema);
			AssertEquals("Script Upgrade Required (no script version diff, and schema upgrade is not required)?", false, iVersionInfo.IsRequired_Script);

			// SCHEMA VERSION: App > DB
			versionInfo.ReInitialiseDbReferenceVersion_Schema(SchemaVersion.Application.Major, SchemaVersion.Application.Minor - 1);
			AssertEquals("Schema Upgrade Required (App > DB and !Major)", true, iVersionInfo.IsRequired_Schema);
			AssertEquals("Script Upgrade Required (no script version diff, but schema upgrade is required)?", true, iVersionInfo.IsRequired_Script);
		}

		public void TestDataVersion()
		{
			DbUpgraderVersionInfoForTesting versionInfo = new DbUpgraderVersionInfoForTesting();
			var iVersionInfo = versionInfo as IVersionChangeInfo;

			// App = DB [PRE-CONDITION]
			AssertEquals("[PRE-CONDITION] Data Version (App = DB)", 0, DataVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Data));
			AssertEquals("[PRE-CONDITION] Data Upgrade Required (App = DB)", false, iVersionInfo.IsRequired_Data);

			// App < DB
			versionInfo.ReInitialiseDbReferenceVersion_Data(DataVersion.Application.Major, DataVersion.Application.Minor + 1);
			AssertEquals("Data Version (App < DB)", true, DataVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Data) < 0);
			AssertEquals("Data Upgrade Required (App < DB)", true, iVersionInfo.IsRequired_Data);

			// App > DB
			versionInfo.ReInitialiseDbReferenceVersion_Data(DataVersion.Application.Major - 1, DataVersion.Application.Minor);
			AssertEquals("Data Version (App > DB)", true, DataVersion.Application.CompareTo(iVersionInfo.DbReferenceVersion_Data) > 0);
			AssertEquals("Data Upgrade Required (App > DB)", true, iVersionInfo.IsRequired_Data);
		}

		public void TestTransformationVersion()
		{
			DbUpgraderVersionInfoForTesting versionInfo = new DbUpgraderVersionInfoForTesting();
			var iVersionInfo = versionInfo as IVersionChangeInfo;

			// App = DB [PRE-CONDITION]
			AssertEquals("[PRE-CONDITION] Transformation Version (App = DB)", 0, iVersionInfo.DbReferenceVersion_Transformation.CompareTo(TransformationVersion.ApplicationNumber));
			AssertEquals("[PRE-CONDITION] Transformation Upgrade Required (App = DB)", false, iVersionInfo.IsRequired_Transformation);

			// App < DB
			versionInfo.ReInitialiseDbReferenceVersion_Transformation(TransformationVersion.ApplicationNumber.Major + 1, TransformationVersion.ApplicationNumber.Minor);
			AssertEquals("Transformation Version (App < DB)", true, TransformationVersion.ApplicationNumber.CompareTo(iVersionInfo.DbReferenceVersion_Transformation) < 0);
			AssertEquals("Transformation Upgrade Required (App < DB)", true, iVersionInfo.IsRequired_Transformation);

			// App > DB
			versionInfo.ReInitialiseDbReferenceVersion_Transformation(TransformationVersion.ApplicationNumber.Major - 1, TransformationVersion.ApplicationNumber.Minor);
			AssertEquals("Transformation Version (App > DB)", true, TransformationVersion.ApplicationNumber.CompareTo(iVersionInfo.DbReferenceVersion_Transformation) > 0);
			AssertEquals("Transformation Upgrade Required (App > DB)", true, iVersionInfo.IsRequired_Transformation);

			Assert("The minor version should be >= 0", TransformationVersion.ApplicationNumber.Minor >= 0);

			if (ZArchitecture.Core.ReleaseInfo.Instance.ReleaseRing == ReleaseRings.Codes.ALP)
			{
				Assert("The minor version is always 0 on the alpha", TransformationVersion.ApplicationNumber.Minor == 0);
			}
		}

		public void TestClrVersion()
		{
			// Arrange
			const int major = -123;
			const int minor = -456;
			Env.Registry.DatabaseMajorClrAssembliesVersion = major;
			Env.Registry.DatabaseMinorClrAssembliesVersion = minor;
			var versionInfo = new DbUpgraderVersionInfoForTesting();

			// Act
			var result = new { versionInfo.DbReferenceVersion_Clr.Major, versionInfo.DbReferenceVersion_Clr.Minor };

			// Assert
			AssertEquals(major, result.Major);
			AssertEquals(minor, result.Minor);
		}

		public void TestIsRequired_ClientDocuments()
		{
			IVersionChangeInfo versionInfo = new DbUpgraderVersionInfo();
			AssertEquals("IsRequired_ClientDocuments should be false when not upgrading", false, versionInfo.IsRequired_ClientDocuments);

			UpgradeInfo softwareUpgrade = new UpgradeInfo(new Guid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			versionInfo = new DbUpgraderVersionInfo(softwareUpgrade);
			AssertEquals("IsRequired_ClientDocuments should be false with no ClientDocumentName registry value set", false, versionInfo.IsRequired_ClientDocuments);

			DataRegistry.Instance.ClientDocumentName = "ABC";

			versionInfo = new DbUpgraderVersionInfo();
			AssertEquals("IsRequired_ClientDocuments should be false when not upgrading", false, versionInfo.IsRequired_ClientDocuments);

			versionInfo = new DbUpgraderVersionInfo(softwareUpgrade);
			AssertEquals("IsRequired_ClientDocuments should be true with upgrade and CLientDocumentName registry value set", true, versionInfo.IsRequired_ClientDocuments);
		}

		public void TestResetTranformVersionWhenUpgradeWithTooLargeTranformVersion()
		{
			UpgradeInfo softwareUpgrade = new UpgradeInfo(new Guid(), ReleaseInfo.Instance.VersionNumber.AddMajor(1).ToVersion());
			var versionInfo = new DbUpgraderVersionInfo(softwareUpgrade);
			AssertEquals(TransformationVersion.ApplicationNumber.ToString(), ((IVersionChangeInfo)versionInfo).DbReferenceVersion_Transformation.ToString());

			DataRegistry.Instance.DatabaseMajorTransformationVersion = DataRegistry.Instance.DatabaseMajorTransformationVersion - 1;
			var actualVersion = new VersionLabel(DataRegistry.Instance.DatabaseMajorTransformationVersion, DataRegistry.Instance.DatabaseMinorTransformationVersion);
			versionInfo = new DbUpgraderVersionInfo(softwareUpgrade);
			AssertEquals(actualVersion.ToString(), ((IVersionChangeInfo)versionInfo).DbReferenceVersion_Transformation.ToString());

			DataRegistry.Instance.DatabaseMajorTransformationVersion = DataRegistry.Instance.DatabaseMajorTransformationVersion + 2;
			actualVersion = new VersionLabel(DataRegistry.Instance.DatabaseMajorTransformationVersion, DataRegistry.Instance.DatabaseMinorTransformationVersion);

			softwareUpgrade = new UpgradeInfo(new Guid(), new Version(1, 4, 5, 6));
			versionInfo = new DbUpgraderVersionInfo(softwareUpgrade);
			AssertEquals(actualVersion.ToString(), ((IVersionChangeInfo)versionInfo).DbReferenceVersion_Transformation.ToString());

			softwareUpgrade = new UpgradeInfo(new Guid(), ReleaseInfo.Instance.VersionNumber.AddMajor(1).ToVersion());
			versionInfo = new DbUpgraderVersionInfo(softwareUpgrade);
			AssertEquals(new VersionLabel(0, 0).ToString(), ((IVersionChangeInfo)versionInfo).DbReferenceVersion_Transformation.ToString());
			AssertEquals(new VersionLabel(0, 0).ToString(), new VersionLabel(DataRegistry.Instance.DatabaseMajorTransformationVersion, DataRegistry.Instance.DatabaseMinorTransformationVersion).ToString());
		}
	}
}
