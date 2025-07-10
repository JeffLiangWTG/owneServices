using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Warehouse
{
	[TestedType(typeof(DateWithinOneYearRegistryDataType))]
	sealed class DateWithinOneYearRegistryDataTypeTest : RegistryDataTypeTestCase<DateWithinOneYearRegistryDataType>
	{
		protected override DateWithinOneYearRegistryDataType GetNewDataType()
		{
			return new DateWithinOneYearRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[] {
				new ValidSampleAndBinaryValueInDB(ZDateTime.Today.ToDateTime(), GetNewDataType().Serialise(ZDateTime.Today.ToDateTime())),
				new ValidSampleAndBinaryValueInDB(ZDateTime.Today.AddDays(-1).ToDateTime(), GetNewDataType().Serialise(ZDateTime.Today.AddDays(-1).ToDateTime())),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { ZDateTime.Today.AddMonths(13).ToDateTime() };
		}

		public void TestValidation()
		{
			var dataType = new DateWithinOneYearRegistryDataType();
			var registryItem = new DateTimeRegistryItem("", null, null, null, RegistryStorageFlags.Company, DateTime.MinValue);
			AssertExceptionThrown(typeof(RegistryValidationException), "You need to enter a date within 12 months", () => dataType.Validate(registryItem, ZDateTime.Now.AddMonths(13).ToDateTime(), Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
