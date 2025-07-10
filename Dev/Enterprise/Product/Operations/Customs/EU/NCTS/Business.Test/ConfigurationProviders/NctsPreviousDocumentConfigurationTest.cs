using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public abstract class NctsPreviousDocumentConfigurationTestCase<T> : TestCaseWithFactory
	where T : NctsPreviousDocumentConfiguration, new()
{
	protected T Configuration { get; private set; }

	public void TestDerivedClassIsSealed()
	{
		if (typeof(T) == typeof(NctsPreviousDocumentConfiguration))
		{
			Assert(true);
		}
		else
		{
			Assert("Derived class should be sealed", typeof(T).IsSealed);
		}
	}

	public void TestGetValidationDecider()
	{
		CombineAssertions(() =>
		{
			AssertType("NCTS4 Departure", ExpectedNctsDeparturePreviousDocumentPhase4ValidationDecider, Configuration.GetValidationDecider(GetDeparturePhase4PreviousDocument()));
			AssertType("NCTS5 Departure", ExpectedNctsDeparturePreviousDocumentPhase5ValidationDecider, Configuration.GetValidationDecider(GetDeparturePhase5PreviousDocument()));
			AssertType("NCTS5 Arrival", ExpectedNctsArrivalPreviousDocumentPhase5ValidationDecider, Configuration.GetValidationDecider(GetArrivalPhase5PreviousDocument()));
		});
	}

	protected abstract Type ExpectedNctsDeparturePreviousDocumentPhase4ValidationDecider { get; }

	protected abstract Type ExpectedNctsDeparturePreviousDocumentPhase5ValidationDecider { get; }

	protected abstract Type ExpectedNctsArrivalPreviousDocumentPhase5ValidationDecider { get; }

	protected override void SetUp()
	{
		base.SetUp();
		Configuration = new();
	}

	NctsPreviousDocument GetDeparturePhase4PreviousDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader.GoodsItems.AddNew().PreviousDocuments.AddNew();
	}

	NctsPreviousDocument GetArrivalPhase5PreviousDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().PreviousDocuments.AddNew();
	}

	NctsPreviousDocument GetDeparturePhase5PreviousDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return nctsHeader.Bills.AddNew().GoodsItems.AddNew().PreviousDocuments.AddNew();
	}
}

[TestedType(typeof(NctsPreviousDocumentConfiguration))]
sealed class NctsPreviousDocumentConfigurationTest : NctsPreviousDocumentConfigurationTestCase<NctsPreviousDocumentConfiguration>
{
	protected override Type ExpectedNctsDeparturePreviousDocumentPhase4ValidationDecider => null;

	protected override Type ExpectedNctsDeparturePreviousDocumentPhase5ValidationDecider => typeof(NctsPreviousDocumentDeparturePhase5ValidationDecider);

	protected override Type ExpectedNctsArrivalPreviousDocumentPhase5ValidationDecider => typeof(NctsPreviousDocumentArrivalPhase5ValidationDecider);
}
