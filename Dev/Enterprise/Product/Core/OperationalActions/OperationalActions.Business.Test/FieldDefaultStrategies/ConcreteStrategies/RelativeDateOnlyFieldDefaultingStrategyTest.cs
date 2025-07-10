using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class RelativeDateOnlyFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		[TestDate(2008, 09, 07, 15, 20, 00)]
		public void TestBehaviour()
		{
			OperationalActionDateFieldSupporter supporter = new OperationalActionDateFieldSupporter("fieldName", false);
			RelativeDateOnlyFieldDefaultingStrategy strategy = new RelativeDateOnlyFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Integer, strategy.DetailFieldType);
			AssertEquals(4, strategy.DetailMaxLength);
			AssertEquals(new ZDateTime(2008, 09, 09, 0, 0, 00), strategy.GetDefaultValue("2"));
			AssertEquals(new ZDateTime(2008, 09, 05, 0, 0, 00), strategy.GetDefaultValue("-2"));
			AssertEquals(ZDateTime.Invalid, strategy.GetDefaultValue(""));
			AssertEquals(ZDateTime.Invalid, strategy.GetDefaultValue(null));
			AssertEquals(ZDateTime.Invalid, strategy.GetDefaultValue("asd"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}
	}
}
