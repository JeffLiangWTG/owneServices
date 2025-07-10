using System;
using System.Linq;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing;

class SRRSpecificValidationTest : TransactionedTestCase
{
	public void TestValidateReturnsEmptyValidationResultIfRegistryValueIsZero()
	{
		// Arrange
		var validation = new SRRSpecificValidation(3);
		using var registry = SystemDataRegistry.Instance.ReportMaxConnections.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals("no error is expected", 0, result.Errors.Count());
		AssertEquals("no warning is expected", 0, result.PropertySpecificWarnings.Count());
	}

	public void TestValidateReturnsEmptyValidationResultIfGivenValueIsLessThanRegistryValue()
	{
		// Arrange
		var validation = new SRRSpecificValidation(2);
		using var registry = SystemDataRegistry.Instance.ReportMaxConnections.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals("no error is expected", 0, result.Errors.Count());
		AssertEquals("no warning is expected", 0, result.PropertySpecificWarnings.Count());
	}

	public void TestValidateReturnsValidationResultWithErrorIfGivenValueEqualsRegistryValue()
	{
		// Arrange
		var validation = new SRRSpecificValidation(3);
		using var registry = SystemDataRegistry.Instance.ReportMaxConnections.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals(1, result.Errors.Count());
		AssertContains("Maximum count of secondary processes must be less than", result.Errors.First());

		AssertEquals(0, result.PropertySpecificWarnings.Count());
	}

	public void TestValidateReturnsValidationResultIfGivenValueIsGreaterThanRegistryValue()
	{
		// Arrange
		var validation = new SRRSpecificValidation(4);
		using var registry = SystemDataRegistry.Instance.ReportMaxConnections.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals(1, result.Errors.Count());
		AssertContains("Maximum count of secondary processes must be less than", result.Errors.First());
		AssertEquals(0, result.PropertySpecificWarnings.Count());
	}
}
