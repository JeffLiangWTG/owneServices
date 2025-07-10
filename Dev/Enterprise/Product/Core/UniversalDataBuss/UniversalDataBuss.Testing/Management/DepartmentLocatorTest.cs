using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Management;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.UniversalDataBuss.Testing.Management
{
	internal class DepartmentLocatorTest : TestCaseWithFactory
	{
		public void TestShouldReturnTrueWhenActiveDepartmentIsDefaultedTo()
		{
			var message = GetMessageForTest();

			var mockContextObject = GetMockContextForTests();
			var locator = new DepartmentLocator(Factory, false) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be true", result.active);
			AssertEquals("AXZ", result.department.GE_Code);
		}

		public void TestShouldReturnFalseWhenInActiveDepartmentIsDefaultedTo()
		{
			var message = GetMessageForTest(false);
			var department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "BRN");
			department.GE_IsActive = false;
			Factory.Save();
			var mockContextObject = GetMockContextForTests();
			var locator = new DepartmentLocator(Factory, false) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be false", !result.active);
			AssertEquals("BRN", result.department.GE_Code);
		}

		public void TestShouldReturnTrueWhenFallbackToDefaultAsSpecifiedDepartmentDoesntExistAndDefaultActive()
		{
			var message = GetMessageForTest();
			var mockContextObject = GetMockContextForTests();
			mockContextObject.SetupGet(contextObject => contextObject.EventDepartmentCode).Returns("ABC");
			var locator = new DepartmentLocator(Factory, false) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be true", result.active);
			AssertEquals("AXZ", result.department.GE_Code);
		}

		public void TestShouldReturnTrueWhenSpecifiedDepartmentIsInactiveAndDefaultIsActive()
		{
			var message = GetMessageForTest(true);
			CreateDepartmentForTest("ABC", false);
			var mockContextObject = GetMockContextForTests();
			mockContextObject.SetupGet(contextObject => contextObject.EventDepartmentCode).Returns("ABC");
			var locator = new DepartmentLocator(Factory, false) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be true", result.active);
			AssertEquals("AXZ", result.department.GE_Code);
		}

		public void TestShouldReturnTrueWhenSpecifiedDepartmentIsActiveAndUpdateMessageDepartmentToSpecified()
		{
			var message = GetMessageForTest(true);
			var specifiedDepartment = CreateDepartmentForTest("ABC");
			var mockContextObject = GetMockContextForTests();
			mockContextObject.SetupGet(contextObject => contextObject.EventDepartmentCode).Returns("ABC");
			var locator = new DepartmentLocator(Factory, false) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be true", result.active);
			AssertEquals("ABC", result.department.GE_Code);
		}

		public void TestShouldReturnActiveAndDefaultDepartmentWhenCodeMapIsFalse()
		{
			var message = GetMessageForTest();
			var specifiedDepartment = CreateDepartmentForTest("ABC");
			var mockContextObject = GetMockContextForTests();
			mockContextObject.SetupGet(contextObject => contextObject.EventDepartmentCode).Returns("ABC");
			mockContextObject.SetupGet(contextObject => contextObject.CodesMappedToTarget).Returns(false);

			var locator = new DepartmentLocator(Factory, false) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be true", result.active);
			AssertEquals("ABC", result.department.GE_Code);

			locator = new DepartmentLocator(Factory, true);
			result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be true", result.active);
			AssertEquals("AXZ", result.department.GE_Code);
		}

		public void TestShouldLogWarningWhenSpecifiedDepartmentIsInactiveAndFallingBackToDefault()
		{
			var message = GetMessageForTest();
			CreateDepartmentForTest("ABC", false);
			var mockContextObject = GetMockContextForTests();
			mockContextObject.SetupGet(contextObject => contextObject.EventDepartmentCode).Returns("ABC");

			var mockLogger = new Mock<ISimpleLogger>();
			mockLogger.Setup(logger => logger.Log(LogType.Warning, It.IsAny<string>())).Verifiable();
			var locator = new DepartmentLocator(Factory, false, mockLogger.Object) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			mockLogger.Verify();
			Assert(true);
		}

		public void TestShouldLogBothWarningWhenSpecifiedDepartmentIsInactiveAndFallingBackToDefaultAndLoggerIsIXMLImportLogger()
		{
			var message = GetMessageForTest();
			CreateDepartmentForTest("ABC", false);
			var mockContextObject = GetMockContextForTests();
			mockContextObject.SetupGet(contextObject => contextObject.EventDepartmentCode).Returns("ABC");

			var mockLogger = new Mock<IXmlImportLogger>();
			mockLogger.Setup(logger => logger.LogBoth(LogType.Warning, It.IsAny<string>())).Verifiable();
			var locator = new DepartmentLocator(Factory, false, mockLogger.Object) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			mockLogger.Verify();
			Assert(true);
		}

		public void TestShouldEvaluateEnvironmentDepartmentWhenDefaultAndSpecifiedAreInActive()
		{
			var message = GetMessageForTest(false);
			var specifiedDepartment = CreateDepartmentForTest("ABC", false);
			var mockContextObject = GetMockContextForTests();
			mockContextObject.SetupGet(contextObject => contextObject.EventDepartmentCode).Returns("ABC");
			var locator = new DepartmentLocator(Factory, false) as IDepartmentLocator;
			var result = locator.TryGetActiveDepartment(message, mockContextObject.Object);
			Assert("result should be true", result.active);
			AssertEquals("BRN", result.department.GE_Code);
		}

		GlbDepartment CreateDepartmentForTest(string departmentCode, bool active = true)
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = departmentCode;
			department.GE_IsActive = active;
			Factory.Save();
			return department;
		}

		EDIMessage GetMessageForTest(bool active = true, string departmentCode = "AXZ")
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();

			var department = CreateDepartmentForTest(departmentCode, active);
			message.EM_GE = department.PK;

			Factory.Save();
			return message;
		}

		Mock<IDataContextDataObject> GetMockContextForTests()
		{
			var mockContextObject = new Mock<IDataContextDataObject>();
			mockContextObject.Setup(contextObject => contextObject.CodesMappedToTarget).Returns(true);
			return mockContextObject;
		}
	}
}
