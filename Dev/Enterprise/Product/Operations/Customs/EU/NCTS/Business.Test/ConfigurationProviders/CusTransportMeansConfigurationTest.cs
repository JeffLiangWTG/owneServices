using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(CusTransportMeansConfiguration))]
public abstract class CusTransportMeansConfigurationAbstractTest<T> : TestCaseWithFactory
	where T : CusTransportMeansConfiguration
{
	public void TestValidationDecider_WhenHeaderIsNull()
	{
		AssertNull(configuration.GetValidationDecider(null));
	}

	public void TestValidationDecider()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType(ExpectedDepartureCusTransportMeansPhase5ValidationDeciderType, configuration.GetValidationDecider(header));
	}

	protected abstract Type ExpectedDepartureCusTransportMeansPhase5ValidationDeciderType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		configuration = (T)Activator.CreateInstance(typeof(T));
		header = Factory.New<NctsHeader>();
	}

	T configuration;
	NctsHeader header;
}

[TestedType(typeof(CusTransportMeansConfiguration))]
sealed class CusTransportMeansConfigurationBaseOnlyTest : CusTransportMeansConfigurationAbstractTest<CusTransportMeansConfiguration>
{
	protected override Type ExpectedDepartureCusTransportMeansPhase5ValidationDeciderType => typeof(DepartureCusTransportMeansPhase5ValidationDecider);
}
