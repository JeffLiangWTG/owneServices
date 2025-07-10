using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccEPaymentDealLookups : AutoAccEPaymentDealLookups
	{
		public AccEPaymentDealLookups(AutoAccEPaymentDeal parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StatusCodeList => EPaymentStatusCodes.Deal.CodesList;

		public CodeDescriptionPairList ProviderCodeList => EPaymentProviderCodes.CodesList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ofx deal status")]
		public static class OFXDealStatus
		{
			public const string Booked = "Booked";
			public const string ReceivedNotCleared = "Received/Not Cleared";
			public const string Received = "Received";
			public const string ReadyForPayment = "Ready For Payment";
			public const string Paid = "Paid";
			public const string Error = "Error";
		}

		#region Quotes

		public virtual AccEPaymentQuoteCollection Quotes
		{
			get { return new AccEPaymentQuoteCollection(Factory); }
		}

		#endregion
	}
}
