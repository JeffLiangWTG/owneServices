using System;
using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing
{
	class DatabaseVersionExceptionTest
	{
		[Test]
		public void TestAllVersionsException()
		{
			// Arrange
			var version = new VersionLabel(0, 0);

			// Act
			// Assert
			var result = Assert.Throws<ArgumentException>(() => new DatabaseVersionException(version, version, version, version, version, version));
			Assert.That(result.Message, Is.EqualTo("All versions are the same."));
		}

		[Test]
		public void TestWrongVersionMessage()
		{
			// Arrange
			var databaseSchemaVersion = new VersionLabel(3680, 21);
			var databaseScriptVersion = new VersionLabel(3816, 48);
			var databaseTransformationVersion = new VersionLabel(2264, 26);
			var applicationSchemaVersion = new VersionLabel(7, 8);
			var applicationScriptVersion = new VersionLabel(9, 10);
			var applicationTransformationVersion = new VersionLabel(11, 12);

			// Act
			var result = new DatabaseVersionException(databaseSchemaVersion, databaseScriptVersion, databaseTransformationVersion, applicationSchemaVersion, applicationScriptVersion, applicationTransformationVersion).Message;

			// Assert
			Assert.That(result, Is.EqualTo(@"The Database Version does not match the Application Version.

Database Schema: 3680.21
Application Schema: 7.8

Database Script: 3816.48
Application Script: 9.10

Database Transformation: 2264.26
Application Transformation: 11.12)

Please contact your system administrator to resolve this."));
		}

		[Test]
		public void TestMayBeInRestoreMessage()
		{
			// Arrange
			var databaseSchemaVersion = new VersionLabel(3680, -1);
			var databaseScriptVersion = new VersionLabel(3816, 48);
			var databaseTransformationVersion = new VersionLabel(2264, 26);
			var applicationSchemaVersion = new VersionLabel(7, 8);
			var applicationScriptVersion = new VersionLabel(9, 10);
			var applicationTransformationVersion = new VersionLabel(11, 12);

			// Act
			var result = new DatabaseVersionException(databaseSchemaVersion, databaseScriptVersion, databaseTransformationVersion, applicationSchemaVersion, applicationScriptVersion, applicationTransformationVersion).Message;

			// Assert
			Assert.That(result, Is.EqualTo(@"The Database Version does not match the Application Version. Database may be in restore state.

Database Schema: 3680.-1
Application Schema: 7.8

Database Script: 3816.48
Application Script: 9.10

Database Transformation: 2264.26
Application Transformation: 11.12)

Please contact your system administrator to resolve this."));
		}

		[Test]
		public void TestInRestoreMessage()
		{
			// Arrange
			var databaseSchemaVersion = new VersionLabel(3680, -1);
			var databaseScriptVersion = new VersionLabel(3816, 48);
			var databaseTransformationVersion = new VersionLabel(2264, 26);
			var applicationSchemaVersion = new VersionLabel(databaseSchemaVersion.Major, 8);
			var applicationScriptVersion = new VersionLabel(databaseScriptVersion.Major, databaseScriptVersion.Minor);
			var applicationTransformationVersion = new VersionLabel(databaseTransformationVersion.Major, databaseTransformationVersion.Minor);

			// Act
			var result = new DatabaseVersionException(databaseSchemaVersion, databaseScriptVersion, databaseTransformationVersion, applicationSchemaVersion, applicationScriptVersion, applicationTransformationVersion).Message;

			// Assert
			Assert.That(result, Is.EqualTo("The Database is in restore state.\r\n\r\nPlease contact your system administrator to resolve this."));
		}
	}
}