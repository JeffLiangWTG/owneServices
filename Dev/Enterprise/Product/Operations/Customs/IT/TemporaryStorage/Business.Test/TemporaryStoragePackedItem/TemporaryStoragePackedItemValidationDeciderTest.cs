using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStoragePackedItemValidationDecider))]
sealed class TemporaryStoragePackedItemValidationDeciderTest : TemporaryStoragePackedItemValidationDeciderAbstractTest<TemporaryStoragePackedItemValidationDecider>
{
	protected override bool ExpectedIsAPI_TariffMandatory => false;
	protected override bool ExpectedIsAPI_GoodsDescriptionMandatory => false;
}
