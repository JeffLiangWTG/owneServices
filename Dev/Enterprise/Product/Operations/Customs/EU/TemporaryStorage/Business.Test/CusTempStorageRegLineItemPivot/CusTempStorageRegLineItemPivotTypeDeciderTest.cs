using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItemPivotTypeDecider))]
sealed class CusTempStorageRegLineItemPivotTypeDeciderTest : TestCase
{
	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineItemPivot), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineItemPivot), Decider.GetTypeForNew());
	}

	public void TestGetRegLineType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetRegLineType());
	}

	public void TestGetRegLineItemType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineItem), Decider.GetRegLineItemType());
	}

	CusTempStorageRegLineItemPivotTypeDecider Decider => decider ??= new();
	CusTempStorageRegLineItemPivotTypeDecider decider;
}
