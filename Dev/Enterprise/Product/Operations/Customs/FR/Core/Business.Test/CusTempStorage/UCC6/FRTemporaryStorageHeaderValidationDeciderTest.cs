using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(FRTemporaryStorageHeaderValidationDecider))]
	sealed class FRTemporaryStorageHeaderValidationDeciderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageHeaderValidationDeciderAbstractTest<FRTemporaryStorageHeaderValidationDecider>
	{
		protected override bool ExpectedIsRule058Active => false;

		protected override bool ExpectedIsPreLodgedStatusCheckActive => true;

		protected override bool ExpectedIsCheckCRNAndMRNForTSAActive => true;

		protected override bool ExpectedIsMessageTypeCheckActive => true;

		protected override bool ExpectedIsAuthorizationUsageCheckActive => true;
	}
}
