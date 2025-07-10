using System;
using CargoWise.Common;
using NUnit.Framework;
using NUnit.Framework.Dat;

namespace Enterprise.Builder.Generator
{
	class DatabaseRestorerTest : TestCase
	{
		public void TestDevelopmentDbBackupPath_OverrideExists_ReturnsOverridePath()
		{
			var restorer = new DatabaseRestorer();
			var expectedPath = "C:\\Backup\\Database.bak";

			using (SetEnvironmentVariableTemporarily("WTG_DevelopmentDbBackupPath", expectedPath))
			{
				var actualPath = restorer.DevelopmentDbBackupPath;

				AssertEquals(expectedPath, actualPath);
			}
		}

		public void TestDevelopmentDbBackupPath_OverrideNotExists_ReturnsDefaultPath()
		{
			var restorer = new DatabaseRestorer();

			using (SetEnvironmentVariableTemporarily("WTG_DevelopmentDbBackupPath", null))
			{
				var actualPath = restorer.DevelopmentDbBackupPath;

				AssertStartsWith("DevelopmentDbBackupPath has changed", DatServerConnection.DatFileSharePath, actualPath);
			}
		}

		IDisposable SetEnvironmentVariableTemporarily(string variable, string value)
		{
			var oldValue = System.Environment.GetEnvironmentVariable(variable);

			System.Environment.SetEnvironmentVariable(variable, value);

			return new DisposableAction(() =>
			{
				System.Environment.SetEnvironmentVariable(variable, oldValue);
			});
		}
	}
}
