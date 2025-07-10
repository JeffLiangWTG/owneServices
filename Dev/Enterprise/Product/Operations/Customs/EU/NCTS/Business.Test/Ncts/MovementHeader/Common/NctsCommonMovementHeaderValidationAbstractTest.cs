using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

abstract class NctsCommonMovementHeaderValidationAbstractTest<TValidationDecider> : BusinessObjectValidationTestCase
	where TValidationDecider : class, INctsMovementHeaderValidationDecider
{
	public void TestCheckBM_InBondEntryType_List()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNctsDeclarationTypeList();
		Factory.Save();
		var movement = GetMovementHeaderForTest();
		ValidationTestHelper.AssertInvalidCodeMessageError(movement.BM_InBondEntryTypeInfo, "XX", NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure);
	}

	public void TestCheckBM_PlaceOfUnloading_List()
	{
		var movement = GetMovementHeaderForTest();
		ValidationTestHelper.AssertInvalidCodeMessageError(movement.BM_PlaceOfUnloadingInfo, "XXZZ!", "GBABY");
	}

	public void TestCheckBM_SealQty_Negative()
	{
		var movement = GetMovementHeaderForTest();
		CombineAssertions(() =>
		{
			movement.BM_SealQty = -1;
			AssertHasErrorContaining("Negative", movement.BM_SealQtyInfo, MandatoryValidation.ValueCannotBeNegative);
			movement.BM_SealQty = 0;
			AssertNoErrorContaining("Not Negative", movement.BM_SealQtyInfo, MandatoryValidation.ValueCannotBeNegative);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		testContext = new MovementHeaderValidationDeciderTestContext<TValidationDecider>(Factory);
	}

	protected override void TearDown()
	{
		testContext?.Dispose();
		base.TearDown();
	}

	protected abstract NctsCommonMovementHeader GetMovementHeaderForTest();
	protected NctsValidationDeciderTestContext<TValidationDecider> testContext;
}
