using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.NumberFountain;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class LocalReferenceNumberStrategyTest : TestCaseWithFactory
{
	[UseSnapshotProtection]
	[TestDate(2022, 03, 03)]
	public void TestGetMessageReferenceNumber()
	{
		var numberStrategy = new LocalReferenceNumberStrategy(Factory);

		AssertEquals("2022EDIDAT000000000001", numberStrategy.GetMessageReferenceNumber());
		AssertEquals("2022EDIDAT000000000002", numberStrategy.GetMessageReferenceNumber());

		TestDateAttribute.AddYears(1);

		AssertEquals("2023EDIDAT000000000001", numberStrategy.GetMessageReferenceNumber());
	}

	[UseSnapshotProtection]
	[TestDate(2022, 09, 05)]
	public void TestGetMessageReferenceNumberDoesNotRollover()
	{
		Env.NumberFountains.ITMessageLocalReferenceNumber(TestDateAttribute.Date.Year.ToString()).SetNext(TestConnection, 999999999998);
		var numberStrategy = new LocalReferenceNumberStrategy(Factory);
		AssertEquals("2022EDIDAT999999999998", numberStrategy.GetMessageReferenceNumber());
		AssertEquals("2022EDIDAT999999999999", numberStrategy.GetMessageReferenceNumber());
		AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() => numberStrategy.GetMessageReferenceNumber());
	}
}
