using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedDateFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestLongDetail()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Long);
			FixedDateFieldDefaultingStrategy strategy = new FixedDateFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.DateTime, strategy.DetailFieldType);
			AssertEquals(15, strategy.DetailMaxLength);
			AssertEquals(new ZDateTime(1981, 02, 05, 10, 30, 00), strategy.GetDefaultValue("05-FEB-81 10:30"));
			AssertEquals(new ZDateTime(2008, 09, 07, 14, 35, 00), strategy.GetDefaultValue("07-SEP-08 14:35"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}

		public void TestShortDetail()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Short);
			FixedDateFieldDefaultingStrategy strategy = new FixedDateFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Date, strategy.DetailFieldType);
			AssertEquals(15, strategy.DetailMaxLength);
			AssertEquals(new ZDateTime(1981, 02, 05), strategy.GetDefaultValue("05-FEB-81"));
			AssertEquals(new ZDateTime(2008, 09, 07), strategy.GetDefaultValue("07-SEP-08"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}

		public void TestInvalidDefaultValue()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Long);
			FixedDateFieldDefaultingStrategy strategy = new FixedDateFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.DateTime, strategy.DetailFieldType);
			AssertEquals(15, strategy.DetailMaxLength);
			AssertNoExceptionThrown(() => strategy.GetDefaultValue("Not a date"));
		}
	}
}
