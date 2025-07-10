using System;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LoggingMethodsDataType))]
	sealed class LoggingMethodsDataTypeTest : CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataTypeTest
	{
		protected override string ExpectedEditorName => "LoggingMethodsRegistryItemEditor";

		public void TestAllowNewIsFalse()
		{
			// Arrange
			var dataType = new LoggingMethodsDataType();

			// Act
			var result = dataType.AllowNew;

			// Assert
			AssertEquals(false, result);
		}

		public void TestDefaultValues()
		{
			// Arrange
			var dataType = new LoggingMethodsDataType();
			var expectedResult = dataType
				.SystemDefinedList
				.Cast<ICodeDescription>()
				.Select(b => (b.Code, b.Description))
				.ToArray();

			// Act
			var result = dataType.SystemDefinedList
				.Cast<ICodeDescription>()
				.Select(b => (b.Code, b.Description));

			// Assert
			AssertContainsExactElementsInAnyOrder(expectedResult, result);
		}

		public void TestDefaultSecondColumnValue_Hosted()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest("SYD");
			EnvProxy.SetIsInternalSystemForTest(false);
			var dataType = new LoggingMethodsDataType();

			// Act
			var result = dataType.ExtraBoolCheckedCodes;

			// Assert
			AssertEquals(true, EnvProxy.IsHostedWithCargowise);
			AssertEquals(false, EnvProxy.IsInternalSystem);
			AssertContainsExactElementsInAnyOrder(new[] { "CFL", "FSL" }, result);
		}

		public void TestDefaultSecondColumnValue_InternalSystem()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);
			EnvProxy.SetIsInternalSystemForTest(true);
			var dataType = new LoggingMethodsDataType();

			// Act
			var result = dataType.ExtraBoolCheckedCodes;

			// Assert
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals(true, EnvProxy.IsInternalSystem);
			AssertContainsExactElementsInAnyOrder(new[] { "CFL", "FSL" }, result);
		}

		public void TestDefaultSecondColumnValue_NotHostedAndNotInternal()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);
			EnvProxy.SetIsInternalSystemForTest(false);
			var dataType = new LoggingMethodsDataType();

			// Act
			var result = dataType.ExtraBoolCheckedCodes;

			// Assert
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals(false, EnvProxy.IsInternalSystem);
			AssertContainsExactElementsInAnyOrder(new[] { "FSL" }, result);
		}

		public void TestDefaultSecondColumnValue_NotHostedAndUndefinedInternal()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);
			EnvProxy.SetIsInternalSystemForTest(null);
			var dataType = new LoggingMethodsDataType();

			// Act
			var result = dataType.ExtraBoolCheckedCodes;

			// Assert
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals(null, EnvProxy.IsInternalSystem);
			AssertContainsExactElementsInAnyOrder(new[] { "FSL" }, result);
		}

		public void TestCannotSaveWithoutValueInSecondColumn()
		{
			// Arrange
			var dataType = new LoggingMethodsDataType();
			var registryItemMock = new Mock<IRegistryItem>();
			var proposedValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(dataType.CodeMaxLength, dataType.SystemDefinedList, false);
			proposedValue.FindElementByCode(dataType.SystemDefinedList.DefaultCode).Bool = true;
			foreach (var extraBoolCheckedCode in dataType.ExtraBoolCheckedCodes)
			{
				proposedValue.FindElementByCode(extraBoolCheckedCode).Bool2 = false;
			}

			// Act
			// Assert
			var result = AssertExceptionThrown<RegistryValidationException>(
				() => dataType.Validate(registryItemMock.Object, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("You must select at least one logging method to 'Write to'", result.Message);
		}

		public void TestSyslogCannotBeTheSource()
		{
			// Arrange
			var dataType = new LoggingMethodsDataType();
			var registryItemMock = new Mock<IRegistryItem>();
			var proposedValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(dataType.CodeMaxLength, dataType.SystemDefinedList, false);
			proposedValue.FindElementByCode(LoggingMethods.SYS).Bool = true;
			proposedValue.FindElementByCode(LoggingMethods.SYS).Bool2 = true;

			// Act
			// Assert
			var result = AssertExceptionThrown<RegistryValidationException>(
				() => dataType.Validate(registryItemMock.Object, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Syslog can't be selected as the 'Read from' logging method", result.Message);
		}

		public void TestCombinedFileSystemCannotBeTheSource()
		{
			// Arrange
			var dataType = new LoggingMethodsDataType();
			var registryItemMock = new Mock<IRegistryItem>();
			var proposedValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(dataType.CodeMaxLength, dataType.SystemDefinedList, false);
			proposedValue.FindElementByCode(LoggingMethods.CFL).Bool = true;
			proposedValue.FindElementByCode(LoggingMethods.CFL).Bool2 = true;

			// Act
			// Assert
			var result = AssertExceptionThrown<RegistryValidationException>(
				() => dataType.Validate(registryItemMock.Object, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Combined File System can't be selected as the 'Read from' logging method", result.Message);
		}

		protected override CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType GetNewDataType()
		{
			return new LoggingMethodsDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var dataType = new LoggingMethodsDataType();
			var collection1 = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(dataType.CodeMaxLength, dataType.SystemDefinedList, false)
			{
				[0] = { Bool = true },
				[1] = { Bool2 = true },
				[2] = { Bool = true },
				[3] = { Bool2 = true },
				[4] = { Bool2 = true },
			};
			var collection2 = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(dataType.CodeMaxLength, dataType.SystemDefinedList, false)
			{
				[0] = { Bool2 = true },
				[1] = { Bool = true },
				[2] = { Bool2 = true },
				[3] = { Bool2 = true },
				[4] = { Bool2 = true },
			};

			return new[] { new ValidSampleAndBinaryValueInDB(collection1, dataType.Serialise(collection1)), new ValidSampleAndBinaryValueInDB(collection2, dataType.Serialise(collection2)) };
		}
	}
}
