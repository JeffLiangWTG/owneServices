using Enterprise.Customs.BR.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogCreateDraftApplicator))]
	class SendCatalogCreateDraftApplicatorTest : BaseSendCatalogApplicatorTest
	{
		protected override string ExpectedConfirmationMessage => "You are about to send a message to create a draft for all Catalogs that have not yet been sent to Customs (Customs Status: empty and Message Status: Not Sent). Do you want to proceed?";
		protected override string ExpectedConfirmationCaption => "Create Draft Confirmation";
		protected override string ExpectedSendingLog => new GoodsCatalogCreateDraftBatchMessageSenderTest().GetExpectedSendingLog();
	}
}
