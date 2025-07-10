using Enterprise.Customs.BR.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogActivateApplicator))]
	class SendCatalogActivateApplicatorTest : BaseSendCatalogApplicatorTest
	{
		protected override string ExpectedConfirmationMessage => "You are about to send a message to activate all draft Catalogs or those not yet sent to Customs (Customs Status: Draft and Message Status: Not Sent or Customs Status: Draft and Message Status: Not Sent or Accepted). Do you want to proceed?";
		protected override string ExpectedConfirmationCaption => "Activate Confirmation";
		protected override string ExpectedSendingLog => new GoodsCatalogActivateBatchMessageSenderTest().GetExpectedSendingLog();
	}
}
