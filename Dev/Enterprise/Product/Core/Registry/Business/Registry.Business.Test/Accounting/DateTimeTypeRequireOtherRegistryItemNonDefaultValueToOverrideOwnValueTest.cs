using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue))]
	sealed class DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValueTest : RegistryDataTypeTestCase<DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue>
	{
		protected override DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue GetNewDataType()
		{
			return new DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue(OverrideDateTimeRegistryItem);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			OverrideDateTimeRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today);

			return new[] {
				new ValidSampleAndBinaryValueInDB(DateTime.Today, GetNewDataType().Serialise(DateTime.Today)),
				new ValidSampleAndBinaryValueInDB(DateTime.Today.AddDays(-1), GetNewDataType().Serialise(DateTime.Today.AddDays(-1))),
			};
		}

		public void TestValidation()
		{
			var dataType = new DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue(OverrideDateTimeRegistryItem);
			var registryItem = new DateTimeRegistryItem("MyRegistryItem", null, (NoResString)"My Registry Item", null, RegistryStorageFlags.All, DateTime.MinValue);
			AssertExceptionThrown(typeof(RegistryValidationException),
				"This 'My Registry Item' registry item value can be overridden only after the 'Test Registry Item' registry item value overridden at the same fallback level.",
				() => dataType.Validate(registryItem, DateTime.Now, Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), "Please select a valid date.", () => dataType.Validate(registryItem, DateTime.MinValue, Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

			// Set related Registry Item value for the current company 
			OverrideDateTimeRegistryItem.SetValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, DateTime.Now);
			AssertNoExceptionThrown("No error when setting our Registry Item value for the current company", () => dataType.Validate(registryItem, DateTime.Now.AddDays(1), Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

			AssertExceptionThrown("Expect error on attempt to set value for any other company",
				typeof(RegistryValidationException),
				"This 'My Registry Item' registry item value can be overridden only after the 'Test Registry Item' registry item value overridden at the same fallback level.",
				() => dataType.Validate(registryItem, DateTime.Now, Guid.NewGuid(), Guid.Empty, Guid.Empty));

			// Set related Registry Item value for the entire system
			OverrideDateTimeRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1));
			AssertNoExceptionThrown("Now we could set valuye to any company without the error",
				() => dataType.Validate(registryItem, DateTime.Now.AddDays(2), Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown("Now we could set valuye to any company without the error",
				() => dataType.Validate(registryItem, DateTime.Now.AddDays(3), Guid.NewGuid(), Guid.Empty, Guid.Empty));
		}

		DateTimeRegistryItem OverrideDateTimeRegistryItem => overrideDateTimeRegistryItem ?? (overrideDateTimeRegistryItem = new DateTimeRegistryItem("TestRegistryItem", null, (NoResString)"Test Registry Item", null, RegistryStorageFlags.Company, DateTime.MinValue));
		DateTimeRegistryItem overrideDateTimeRegistryItem;
	}
}
