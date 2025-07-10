using Enterprise.Customs.BR.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogCreateNewVersionApplicator))]
	class SendCatalogCreateNewVersionApplicatorTest : BaseSendCatalogApplicatorTest
	{
		protected override string ExpectedConfirmationMessage => "You are about to send a message to create a new version for all active Catalogs (Customs Status: Active and Message Status: Not Sent). Do you want to proceed?”";
		protected override string ExpectedConfirmationCaption => "Create New Version Confirmation";
		protected override string ExpectedSendingLog => new GoodsCatalogCreateNewVersionBatchMessageSenderTest().GetExpectedSendingLog();
	}
}
