using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStoragePackedItemValidationDecider))]
sealed class TemporaryStoragePackedItemValidationDeciderTest : TemporaryStoragePackedItemValidationDeciderAbstractTest<TemporaryStoragePackedItemValidationDecider>
{
	protected override bool ExpectedIsAPI_TariffMandatory => true;
	protected override bool ExpectedIsAPI_GoodsDescriptionMandatory => true;
}
