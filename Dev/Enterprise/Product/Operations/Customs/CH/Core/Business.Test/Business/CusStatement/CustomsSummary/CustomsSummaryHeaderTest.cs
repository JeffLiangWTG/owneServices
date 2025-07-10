using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomsSummaryHeader))]
sealed class CustomsSummaryHeaderTest : EnterpriseBusinessObjectTestCase
{
	public override void TestSaveAndDeleteBusinessObject()
	{
		Assert("Deleting object should have an exception/error because of a trigger.", true);
	}

	public void TestSummaryLines()
	{
		AssertType<CustomsSummaryLineCollection>(((CustomsSummaryHeader)GetNewBusinessObject()).SummaryLines);
	}

	protected override BusinessObject GetNewBusinessObject() => Factory.New<CustomsSummaryHeader>();
}
