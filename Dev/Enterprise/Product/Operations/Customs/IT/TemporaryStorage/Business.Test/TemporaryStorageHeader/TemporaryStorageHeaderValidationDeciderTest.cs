using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageHeaderValidationDecider))]
sealed class TemporaryStorageHeaderValidationDeciderTest : TemporaryStorageHeaderValidationDeciderAbstractTest<TemporaryStorageHeaderValidationDecider>
{
	protected override bool ExpectedIsRule058Active => false;

	protected override bool ExpectedIsPreLodgedStatusCheckActive => false;

	protected override bool ExpectedIsCheckCRNAndMRNForTSAActive => false;

	protected override bool ExpectedIsMessageTypeCheckActive => false;

	protected override bool ExpectedIsAuthorizationUsageCheckActive => false;
}
