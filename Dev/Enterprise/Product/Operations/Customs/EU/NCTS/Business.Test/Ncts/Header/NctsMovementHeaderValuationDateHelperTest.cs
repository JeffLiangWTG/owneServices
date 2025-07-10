using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsMovementHeaderValuationDateHelperTest : TestCaseWithFactory
{
	public void TestSetValuationDate_ArgumentNull()
	{
		AssertExceptionThrown<ArgumentNullException>("When movementHeader is null", () => NctsMovementHeaderValuationDateHelper.SetValuationDate(null, default));
	}

	public void TestSetValuationDate_Phase4()
	{
#if NETFRAMEWORK
		const string expectedExceptionMessage = "Only Phase 5 is supported\r\nParameter name: movementHeader";
#else
		const string expectedExceptionMessage = "Only Phase 5 is supported (Parameter 'movementHeader')";
#endif

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		AssertExceptionThrown<ArgumentException>("Not Phase5", expectedExceptionMessage, () => NctsMovementHeaderValuationDateHelper.SetValuationDate(header.MovementHeader, default));
	}

	[TestDate(2024, 11, 12)]
	public void TestSetValuationDate()
	{
		var testCases = new[]
		{
			(initialValuationTime: ZDateTime.Empty, isAmending: false, expectedValuationDate: ZDateTime.Now),
			(initialValuationTime: ZDateTime.Empty, isAmending: true, expectedValuationDate: ZDateTime.Now),
			(initialValuationTime: ZDateTime.Now.AddDays(-1), isAmending: false, expectedValuationDate: ZDateTime.Now),
			(initialValuationTime: ZDateTime.Now.AddDays(-1), isAmending: true, expectedValuationDate: ZDateTime.Now.AddDays(-1))
		};

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		foreach (var (initialValuationTime, isAmending, expectedValuationDate) in testCases)
		{
			header.MovementHeader.BM_ValuationDate = initialValuationTime;

			NctsMovementHeaderValuationDateHelper.SetValuationDate(header.MovementHeader, isAmending);

			AssertEquals(expectedValuationDate, header.MovementHeader.BM_ValuationDate);
		}
	}
}
