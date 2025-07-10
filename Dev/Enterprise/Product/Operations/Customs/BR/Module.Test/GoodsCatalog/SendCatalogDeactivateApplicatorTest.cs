using Enterprise.Customs.BR.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogDeactivateApplicator))]
	class SendCatalogDeactivateApplicatorTest : BaseSendCatalogApplicatorTest
	{
		protected override string ExpectedConfirmationMessage => "You are about to send a message to deactivate all active Catalogs (Customs Status: Active and Message Status: Accepted). Do you want to proceed?”";
		protected override string ExpectedConfirmationCaption => "Deactivate Confirmation";
		protected override string ExpectedSendingLog => new GoodsCatalogDeactivateBatchMessageSenderTest().GetExpectedSendingLog();
	}
}
