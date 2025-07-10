using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.SG
{
	public class CustomsEntryStatusList : CodeDescriptionPairList
	{
		public CustomsEntryStatusList()
		{
			AddPair(EntryStatus.NotSentForFilter, ResString.GetMultilingualString("CustomsEntryStatusList|NOT", "Not Sent"));
			AddPair(Core.SGConstants.DeclarationStatus.DeclarationPending, ResString.GetMultilingualString("CustomsEntryStatusList|DPD", "Pending Declaration Queued."));
			AddPair(Core.SGConstants.DeclarationStatus.DeclarationSent, ResString.GetMultilingualString("CustomsEntryStatusList|DSN", "Declaration Sent Waiting Response."));
			AddPair(Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms, ResString.GetMultilingualString("CustomsEntryStatusList|DRJ", "Declaration Rejected."));
			AddPair(Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors, ResString.GetMultilingualString("CustomsEntryStatusList|DER", "Declaration in Error."));
			AddPair(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, ResString.GetMultilingualString("CustomsEntryStatusList|DOK", "Permit Approved."));
			AddPair(Core.SGConstants.DeclarationStatus.DeclarationQuery, ResString.GetMultilingualString("CustomsEntryStatusList|DQY", "Declaration Queried."));
			AddPair(Core.SGConstants.DeclarationStatus.AmendmentPending, ResString.GetMultilingualString("CustomsEntryStatusList|APD", "Pending Amendment Declaration Queued."));
			AddPair(Core.SGConstants.DeclarationStatus.AmendmentSent, ResString.GetMultilingualString("CustomsEntryStatusList|ASN", "Amendment Declaration Sent Waiting Response."));
			AddPair(Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms, ResString.GetMultilingualString("CustomsEntryStatusList|ARJ", "Amendment Declaration Rejected."));
			AddPair(Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors, ResString.GetMultilingualString("CustomsEntryStatusList|AER", "Amendment Declaration in Error."));
			AddPair(Core.SGConstants.DeclarationStatus.AmendmentPermitReceived, ResString.GetMultilingualString("CustomsEntryStatusList|AOK", "Amendment Permit Approved."));
			AddPair(Core.SGConstants.DeclarationStatus.AmendmentQuery, ResString.GetMultilingualString("CustomsEntryStatusList|AQY", "Amendment Queried."));
			AddPair(Core.SGConstants.DeclarationStatus.CancellationPending, ResString.GetMultilingualString("CustomsEntryStatusList|CPD", "Pending Cancellation Queued."));
			AddPair(Core.SGConstants.DeclarationStatus.CancellationSent, ResString.GetMultilingualString("CustomsEntryStatusList|CSN", "Cancellation Sent Waiting Response."));
			AddPair(Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms, ResString.GetMultilingualString("CustomsEntryStatusList|CRJ", "Cancellation Rejected."));
			AddPair(Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors, ResString.GetMultilingualString("CustomsEntryStatusList|CER", "Cancellation in Error."));
			AddPair(Core.SGConstants.DeclarationStatus.CancellationAccepted, ResString.GetMultilingualString("CustomsEntryStatusList|COK", "Cancellation Accepted."));
			AddPair(Core.SGConstants.DeclarationStatus.CancellationQuery, ResString.GetMultilingualString("CustomsEntryStatusList|CQY", "Cancellation Queried."));
			AddPair(Core.SGConstants.DeclarationStatus.RefundPending, ResString.GetMultilingualString("CustomsEntryStatusList|RPD", "Pending Refund Request Queued."));
			AddPair(Core.SGConstants.DeclarationStatus.RefundSent, ResString.GetMultilingualString("CustomsEntryStatusList|RSN", "Refund Request Sent Waiting Response."));
			AddPair(Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms, ResString.GetMultilingualString("CustomsEntryStatusList|RRJ", "Refund Request Rejected."));
			AddPair(Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors, ResString.GetMultilingualString("CustomsEntryStatusList|RER", "Refund Request Error."));
			AddPair(Core.SGConstants.DeclarationStatus.RefundPermitReceived, ResString.GetMultilingualString("CustomsEntryStatusList|ROK", "Refund Request Approved."));
			AddPair(Core.SGConstants.DeclarationStatus.RefundQuery, ResString.GetMultilingualString("CustomsEntryStatusList|RQY", "Refund Queried."));
		}
	}

	public static class EntryStatus
	{
		public const string NotSentForFilter = "NOT";
	}
}
