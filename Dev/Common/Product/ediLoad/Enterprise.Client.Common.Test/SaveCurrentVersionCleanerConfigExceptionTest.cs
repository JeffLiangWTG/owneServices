using CargoWise.ApplicationManager.Common;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Client.Common.Testing
{
	class SaveCurrentVersionCleanerConfigExceptionTest : TestCase
	{
		public void TestMessageDefault()
		{
			var result = new SaveCurrentVersionCleanerConfigException().Message;
			AssertEquals("Save CurrentVersionConfig file exception.", result);
		}

		public void TestMessagesOnAppManagerResults()
		{
			var appManagerResults = new AppManagerResult[]
			{
				new AppManagerResult(AppManagerResultStatus.Error, "error result"),
				new AppManagerResult(AppManagerResultStatus.Retry, "retry result"),
				new AppManagerResult(AppManagerResultStatus.TimedOut, "timeout result"),
				new AppManagerResult(AppManagerResultStatus.WaitingForUpgrade, "wait for upgrade"),
				new AppManagerResult(AppManagerResultStatus.Success, "no error"),
			};

			using (var tempDir = new TempDirectory())
			{
				foreach (var appManagerResult in appManagerResults)
				{
					var expectedResult = $"Save CurrentVersionConfig file exception at path: '{tempDir.DirectoryName}', result: {appManagerResult}.";
					var result = new SaveCurrentVersionCleanerConfigException(tempDir.DirectoryName, appManagerResult).Message;
					AssertEquals(expectedResult, result);
				}
			}
		}
	}
}
