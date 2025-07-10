using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class RelativeDateFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		[TestDate(2008, 09, 07, 15, 20, 00)]
		public void TestLongDetail()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Long);
			RelativeDateFieldDefaultingStrategy strategy = new RelativeDateFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Integer, strategy.DetailFieldType);
			AssertEquals(4, strategy.DetailMaxLength);
			AssertEquals(new ZDateTime(2008, 09, 09, 15, 20, 00), strategy.GetDefaultValue("2"));
			AssertEquals(new ZDateTime(2008, 09, 05, 15, 20, 00), strategy.GetDefaultValue("-2"));
			AssertEquals(ZDateTime.Invalid, strategy.GetDefaultValue(""));
			AssertEquals(ZDateTime.Invalid, strategy.GetDefaultValue(null));
			AssertEquals(ZDateTime.Invalid, strategy.GetDefaultValue("asd"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}

		[TestDate(2008, 09, 07, 15, 20, 00)]
		public void TestShortDetail()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Short);
			RelativeDateFieldDefaultingStrategy strategy = new RelativeDateFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Integer, strategy.DetailFieldType);
			AssertEquals(4, strategy.DetailMaxLength);
			AssertEquals(new ZDateTime(2008, 09, 09), strategy.GetDefaultValue("2"));
			AssertEquals(new ZDateTime(2008, 09, 05), strategy.GetDefaultValue("-2"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}
	}
}
