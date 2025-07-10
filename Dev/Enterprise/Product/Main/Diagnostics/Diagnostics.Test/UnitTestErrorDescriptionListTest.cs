using System;
using System.IO;
using CargoWise.Common;
using Enterprise.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.DevTools.Common;

namespace Enterprise.Main.Diagnostics.Testing
{
	sealed class UnitTestErrorDescriptionListTest : TestCase
	{
		public void TestGetFinalTextWhenGetTestSequenceFileLinkThrowException()
		{
			// Arrange
			var result = string.Empty;
			var testList = "Enterprise.Main.Testing,Enterprise.Main.Testing.UnitTestErrorDescriptionListTest,TestGetTestSequenceFileLinkEvenUploadError";
			var expectedInfo = @"<font style=""font-size:8pt""><strong>Test Sequence Information</strong></font><br><br>
<div style=""font-size:7pt; width: 2000px; height: 400px; overflow: auto;""><p>Enterprise.Main.Testing,Enterprise.Main.Testing.UnitTestErrorDescriptionListTest,TestGetTestSequenceFileLinkEvenUploadError</p></div>";
			var testFailureDataClient = new Mock<ITestFailureDataClient>();
			testFailureDataClient.Setup(client => client.Upload(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Simulated upload failure"));
			var unitTestErrorDescriptionListTestFixture = new Mock<UnitTestErrorDescriptionListTestFixture>(testFailureDataClient.Object);
			unitTestErrorDescriptionListTestFixture.Protected().Setup<string>("GetTestSequenceFile").Returns(testList);
			var testInstance = unitTestErrorDescriptionListTestFixture.Object;

			// Act
			var isRunningOnDAT = TestingState.IsRunningOnDAT;
			TestingState.IsRunningOnDAT = true;
			using (new DisposableAction(() => TestingState.IsRunningOnDAT = isRunningOnDAT))
			{
				result = testInstance.GetFinalText();
			}

			// Assert
			AssertContains(expectedInfo, result);
		}
	}

	[Serializable]
	public class UnitTestErrorDescriptionListTestFixture : UnitTestErrorDescriptionList
	{
		public UnitTestErrorDescriptionListTestFixture(ITestFailureDataClient testFailureDataClient) : base(testFailureDataClient)
		{ }

		public string GetFinalText()
		{
			return base.GetFinalText(null);
		}
	}
}
