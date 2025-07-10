using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedDateTimeOffsetFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestDetail()
		{
			OperationalActionDateTimeOffsetFieldSupporter supporter = new OperationalActionDateTimeOffsetFieldSupporter("fieldName", false);
			FixedDateTimeOffsetFieldDefaultingStrategy strategy = new FixedDateTimeOffsetFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.DateTimeOffset, strategy.DetailFieldType);
			AssertEquals(15, strategy.DetailMaxLength);
			AssertEquals(new ZDateTimeOffset(1981, 02, 05, 10, 30, 00, TimeSpan.FromHours(10)), strategy.GetDefaultValue("05-FEB-81 10:30"));
			AssertEquals(new ZDateTimeOffset(2008, 09, 07, 14, 35, 00, TimeSpan.FromHours(10)), strategy.GetDefaultValue("07-SEP-08 14:35"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}

		public void TestInvalidDefaultValue()
		{
			OperationalActionDateTimeOffsetFieldSupporter supporter = new OperationalActionDateTimeOffsetFieldSupporter("fieldName", false);
			FixedDateTimeOffsetFieldDefaultingStrategy strategy = new FixedDateTimeOffsetFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.DateTimeOffset, strategy.DetailFieldType);
			AssertEquals(15, strategy.DetailMaxLength);
			AssertNoExceptionThrown(() => strategy.GetDefaultValue("Not a date"));
		}
	}
}
