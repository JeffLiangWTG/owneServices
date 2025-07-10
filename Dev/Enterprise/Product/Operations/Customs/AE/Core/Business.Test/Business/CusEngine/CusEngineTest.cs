using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusEngine))]
sealed class CusEngineTest : CusEngineAbstractTest
{
	public void TestCEG_EngineNumber() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(engine.CEG_EngineNumberInfo);
		AssertEquals("Engine Number", info.Caption);
		AssertEquals("Engine No.", info.ShortCaption);
	});

	public void TestCEG_CapacityCC() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(engine.CEG_CapacityCCInfo);
		AssertEquals("Engine Capacity (L)", info.Caption);
		AssertEquals("Engine Capacity", info.MediumCaption);
		AssertEquals("Engine Cap.", info.ShortCaption);
	});

	protected override Type ExpectedLookupsType => typeof(CusEngineLookups);

	protected override Type ExpectedValidationType => typeof(CusEngineValidation);

	protected override void SetUp()
	{
		base.SetUp();
		engine = Factory.New<CusEngine>();
	}

	CusEngine engine;
}
