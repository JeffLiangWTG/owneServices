using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(CusSealConfiguration))]
public abstract class CusSealConfigurationAbstractTest<T> : TestCaseWithFactory
	where T : CusSealConfiguration
{
	public void TestValidationDecider_WhenHeaderIsNull()
	{
		AssertNull(configuration.GetValidationDecider(null));
	}

	public void TestValidationDecider_Phase4()
	{
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType(ExpectedCusSealValidationDeciderType, configuration.GetValidationDecider(header));
	}

	public void TestValidationDecider_Phase5()
	{
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType(ExpectedCusSealPhase5ValidationDeciderType, configuration.GetValidationDecider(header));
	}

	protected abstract Type ExpectedCusSealPhase5ValidationDeciderType { get; }

	protected abstract Type ExpectedCusSealValidationDeciderType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		configuration = (T)Activator.CreateInstance(typeof(T));
		header = Factory.New<NctsHeader>();
	}

	T configuration;
	NctsHeader header;
}

[TestedType(typeof(CusSealConfiguration))]
sealed class CusSealConfigurationBaseOnlyTest : CusSealConfigurationAbstractTest<CusSealConfiguration>
{
	protected override Type ExpectedCusSealPhase5ValidationDeciderType => typeof(CusSealPhase5ValidationDecider);

	protected override Type ExpectedCusSealValidationDeciderType => typeof(CusSealValidationDecider);
}
