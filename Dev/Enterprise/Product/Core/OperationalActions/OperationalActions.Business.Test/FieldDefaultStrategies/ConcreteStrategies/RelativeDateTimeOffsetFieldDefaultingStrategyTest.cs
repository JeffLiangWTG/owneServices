using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class RelativeDateTimeOffsetFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2008, 09, 07, 15, 20, 00)]
		public void TestDetail()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals(new ZDateTime(2008, 09, 08, 1, 20, 00), ZDateTime.Now);
			AssertEquals(new ZDateTime(2008, 09, 07, 15, 20, 00), ZDateTime.UtcNow);
			OperationalActionDateTimeOffsetFieldSupporter supporter = new OperationalActionDateTimeOffsetFieldSupporter("fieldName", false);
			RelativeDateTimeOffsetFieldDefaultingStrategy strategy = new RelativeDateTimeOffsetFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Integer, strategy.DetailFieldType);
			AssertEquals(4, strategy.DetailMaxLength);
			AssertEquals(new ZDateTimeOffset(2008, 09, 10, 1, 20, 00, TimeSpan.FromHours(10)), strategy.GetDefaultValue("2"));
			AssertEquals(new ZDateTimeOffset(2008, 09, 06, 1, 20, 00, TimeSpan.FromHours(10)), strategy.GetDefaultValue("-2"));
			AssertEquals(ZDateTimeOffset.Invalid, strategy.GetDefaultValue(""));
			AssertEquals(ZDateTimeOffset.Invalid, strategy.GetDefaultValue(null));
			AssertEquals(ZDateTimeOffset.Invalid, strategy.GetDefaultValue("asd"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}
	}
}
