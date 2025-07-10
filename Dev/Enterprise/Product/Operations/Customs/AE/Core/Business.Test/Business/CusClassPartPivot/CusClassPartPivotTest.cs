using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusClassPartPivot))]
sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestDefaultDataGroupingCode()
	{
		var pivot = Factory.New<CusClassPartPivot>();
		AssertEquals("Data grouping", AEConstants.DefaultDataGroupingForTariffs, pivot.DefaultDataGroupingForTariffs);
	}
}
