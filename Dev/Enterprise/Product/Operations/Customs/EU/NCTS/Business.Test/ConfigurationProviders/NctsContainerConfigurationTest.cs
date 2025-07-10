using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(NctsContainerConfiguration))]
public abstract class NctsContainerConfigurationAbstractTest<T> : TestCaseWithFactory
	where T : NctsContainerConfiguration
{
	public void TestGetNctsHeaderContainerValidationDecider()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_HeaderType = "X";
		header.BH_ApplicationCode = "X";
		AssertNull(configuration.GetHeaderValidationDecider(header));
	}

	public void TestGetNctsArrivalHeaderContainerPhase4ValidationDecider()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertNull(configuration.GetHeaderValidationDecider(header));
	}

	public void TestGetNctsArrivalHeaderContainerPhase5ValidationDecider()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType(NctsArrivalHeaderContainerPhase5ValidationDecider, configuration.GetHeaderValidationDecider(header));
	}

	public void TestGetNctsDepartureHeaderContainerPhase4ValidationDecider()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertNull(configuration.GetHeaderValidationDecider(header));
	}

	public void TestGetNctsDepartureHeaderContainerPhase5ValidationDecider()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType(NctsDepartureHeaderContainerPhase5ValidationDecider, configuration.GetHeaderValidationDecider(header));
	}

	public void TestValidationDecider_WhenHeaderIsNull()
	{
		AssertType(null, configuration.GetHeaderValidationDecider(null));
	}

	protected virtual Type NctsArrivalHeaderContainerPhase5ValidationDecider => typeof(NctsArrivalHeaderContainerPhase5ValidationDecider);

	protected virtual Type NctsDepartureHeaderContainerPhase5ValidationDecider => typeof(NctsDepartureHeaderContainerPhase5ValidationDecider);

	protected override void SetUp()
	{
		base.SetUp();
		configuration = (T)Activator.CreateInstance(typeof(T));
	}
	T configuration;
}

[TestedType(typeof(NctsContainerConfiguration))]
sealed class NctsContainerConfigurationBaseOnlyTest : NctsContainerConfigurationAbstractTest<NctsContainerConfiguration>
{
}
