using System;
using System.Data;
using CargoWise.EntityFramework.Testing;
using Moq;
using ServiceManager.Common.CW;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

class NativeServiceTaskDTOTest : TestCaseWithFactory
{
	public void TestCreateFromReader()
	{
		// Arrange
		var taskPk = Guid.NewGuid();
		var branchPk = Guid.NewGuid();
		var expectedCode = "XYZ";
		var expectedIsActive = true;
		var expectedNextRunTime = DateTimeOffset.Now.AddHours(1);
		var expectedLastRunTime = DateTimeOffset.Now.AddHours(-1);
		var expectedBranchName = "BranchXYZ";
		var expectedSettingsXml = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"2\"/></ScheduleConfig>";

		var mockReader = new Mock<IDataReader>();
		mockReader.Setup(r => r["SST_ServiceTaskCode"]).Returns(expectedCode);
		mockReader.Setup(r => r["SST_PK"]).Returns(taskPk);
		mockReader.Setup(r => r["SST_Active"]).Returns(expectedIsActive);
		mockReader.Setup(r => r["SST_NextRunTime"]).Returns(expectedNextRunTime);
		mockReader.Setup(r => r["SST_LastRunTime"]).Returns(expectedLastRunTime);
		mockReader.Setup(r => r["GB_PK"]).Returns(branchPk);
		mockReader.Setup(r => r["GB_Code"]).Returns(expectedBranchName);
		mockReader.Setup(r => r["SST_Configuration"]).Returns(expectedSettingsXml);

		// Act
		var result = NativeServiceTaskDTO.CreateFromReader(mockReader.Object);

		// Assert
		CombineAssertions(() =>
		{
			AssertNotNull(result);
			AssertEquals(expectedCode, result.ServiceTaskCode);
			AssertEquals(taskPk, result.Pk);
			AssertEquals(expectedIsActive, result.IsActive);
			AssertEquals(expectedNextRunTime, result.NextRunTime);
			AssertEquals(expectedLastRunTime, result.LastRunTime);
			AssertEquals(branchPk, result.BranchPk);
			AssertEquals(expectedBranchName, result.BranchName);
			AssertEquals(string.Empty, result.BranchErrorMessage);
			AssertEquals(expectedSettingsXml, result.SettingsXml);
		});
	}

	public void TestCreateFromReader_WithNullValues()
	{
		// Arrange
		var taskPk = Guid.NewGuid();
		var expectedCode = "XYZ";
		var expectedIsActive = true;
		var expectedNextRunTime = DateTimeOffset.Now.AddHours(1);
		var expectedSettingsXml = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"2\"/></ScheduleConfig>";

		var mockReader = new Mock<IDataReader>();
		mockReader.Setup(r => r["SST_ServiceTaskCode"]).Returns(expectedCode);
		mockReader.Setup(r => r["SST_PK"]).Returns(taskPk);
		mockReader.Setup(r => r["SST_Active"]).Returns(expectedIsActive);
		mockReader.Setup(r => r["SST_NextRunTime"]).Returns(expectedNextRunTime);
		mockReader.Setup(r => r["SST_LastRunTime"]).Returns(DBNull.Value);
		mockReader.Setup(r => r["GB_PK"]).Returns(DBNull.Value);
		mockReader.Setup(r => r["GB_Code"]).Returns(DBNull.Value);
		mockReader.Setup(r => r["SST_Configuration"]).Returns(expectedSettingsXml);

		// Act
		var result = NativeServiceTaskDTO.CreateFromReader(mockReader.Object);

		// Assert
		CombineAssertions(() =>
		{
			AssertNotNull(result);
			AssertEquals(expectedCode, result.ServiceTaskCode);
			AssertEquals(taskPk, result.Pk);
			AssertEquals(expectedIsActive, result.IsActive);
			AssertEquals(expectedNextRunTime, result.NextRunTime);
			AssertNull(result.LastRunTime);
			AssertNull(result.BranchPk);
			AssertEquals(string.Empty, result.BranchName);
			AssertEquals(string.Empty, result.BranchErrorMessage);
			AssertEquals(expectedSettingsXml, result.SettingsXml);
		});
	}
}
