using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(NctsSupportingDocumentConfiguration))]
public abstract class NctsSupportingDocumentConfigurationTestCase<T> : TestCaseWithFactory
			where T : NctsSupportingDocumentConfiguration, new()
{
	protected T Configuration { get; private set; }

	public void TestDerivedClassIsSealed()
	{
		if (typeof(T) == typeof(NctsSupportingDocumentConfiguration))
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
			AssertType("NCTS4 Departure", ExpectedNctsDepartureSupportingDocumentPhase4ValidationDecider, Configuration.GetValidationDecider(GetDeparturePhase4SupportingDocument()));
			AssertType("NCTS5 Departure", ExpectedNctsDepartureSupportingDocumentPhase5ValidationDecider, Configuration.GetValidationDecider(GetDeparturePhase5SupportingDocument()));
			AssertType("NCTS5 Arrival", ExpectedNctsArrivalSupportingDocumentPhase5ValidationDecider, Configuration.GetValidationDecider(GetArrivalPhase5SupportingDocument()));
		});
	}

	protected abstract Type ExpectedNctsDepartureSupportingDocumentPhase4ValidationDecider { get; }

	protected abstract Type ExpectedNctsDepartureSupportingDocumentPhase5ValidationDecider { get; }

	protected abstract Type ExpectedNctsArrivalSupportingDocumentPhase5ValidationDecider { get; }

	protected override void SetUp()
	{
		base.SetUp();
		Configuration = new();
	}

	NctsSupportingDocument GetDeparturePhase4SupportingDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader.GoodsItems.AddNew().SupportingDocuments.AddNew();
	}

	NctsSupportingDocument GetArrivalPhase5SupportingDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().SupportingDocuments.AddNew();
	}

	NctsSupportingDocument GetDeparturePhase5SupportingDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return nctsHeader.Bills.AddNew().GoodsItems.AddNew().SupportingDocuments.AddNew();
	}
}

[TestedType(typeof(NctsSupportingDocumentConfiguration))]
sealed class NctsSupportingDocumentConfigurationTest : NctsSupportingDocumentConfigurationTestCase<NctsSupportingDocumentConfiguration>
{
	protected override Type ExpectedNctsDepartureSupportingDocumentPhase4ValidationDecider => null;

	protected override Type ExpectedNctsDepartureSupportingDocumentPhase5ValidationDecider => typeof(NctsSupportingDocumentDeparturePhase5ValidationDecider);

	protected override Type ExpectedNctsArrivalSupportingDocumentPhase5ValidationDecider => null;
}
