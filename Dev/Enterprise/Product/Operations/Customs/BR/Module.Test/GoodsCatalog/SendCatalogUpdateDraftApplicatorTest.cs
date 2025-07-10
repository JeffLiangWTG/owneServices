using Enterprise.Customs.BR.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogUpdateDraftApplicator))]
	class SendCatalogUpdateDraftApplicatorTest : BaseSendCatalogApplicatorTest
	{
		protected override string ExpectedConfirmationMessage => "You are about to update all draft Catalogs that have changes pending (Customs Status: Draft and Message Status: Not Sent). Do you want to proceed?";
		protected override string ExpectedConfirmationCaption => "Update Draft Confirmation";
		protected override string ExpectedSendingLog => new GoodsCatalogUpdateDraftBatchMessageSenderTest().GetExpectedSendingLog();
	}
}
