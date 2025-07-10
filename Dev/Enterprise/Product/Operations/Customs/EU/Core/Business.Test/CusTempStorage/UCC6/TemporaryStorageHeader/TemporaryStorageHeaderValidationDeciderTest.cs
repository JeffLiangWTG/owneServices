using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeaderValidationDecider))]
	sealed class TemporaryStorageHeaderValidationDeciderTest : TemporaryStorageHeaderValidationDeciderAbstractTest<TemporaryStorageHeaderValidationDecider>
	{
		protected override bool ExpectedIsRule058Active => true;

		protected override bool ExpectedIsPreLodgedStatusCheckActive => true;

		protected override bool ExpectedIsCheckCRNAndMRNForTSAActive => true;

		protected override bool ExpectedIsMessageTypeCheckActive => true;

		protected override bool ExpectedIsAuthorizationUsageCheckActive => true;
	}
}
