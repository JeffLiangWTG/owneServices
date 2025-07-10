using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocViewCommissionLineGrouping : DocBaseWrapper
	{
		protected DocViewCommissionLineGrouping(ViewCommissionLineGrouping grouping, BusinessObjectFactory factory)
			: base(grouping, factory)
		{
		}

		public static DocViewCommissionLineGrouping New(ViewCommissionLineGrouping grouping, BusinessObjectFactory factory)
		{
			return (grouping != null) ? new DocViewCommissionLineGrouping(grouping, factory) : null;
		}

		new ViewCommissionLineGrouping WrappedObject
		{
			get { return (ViewCommissionLineGrouping)base.WrappedObject; }
		}

		public ZString CompanyCode
		{
			get { return WrappedObject.Company != null ? WrappedObject.Company.GC_Code : ZString.Empty; }
		}

		public ZString SourceNumber
		{
			get { return WrappedObject.SourceNumber; }
		}

		public ZDecimal EntityCommissionAmountInLocalCurrency
		{
			get { return WrappedObject.EntityCommissionAmountInLocalCurrency; }
		}

		public ZDecimal EntityCommissionAmountInPreferredCurrency
		{
			get { return WrappedObject.EntityCommissionAmountInPreferredCurrency; }
		}

		public ZString ClientCode
		{
			get { return WrappedObject.SourceClient != null ? WrappedObject.SourceClient.OH_Code : ZString.Empty; }
		}

		public ZString ClientName
		{
			get { return WrappedObject.SourceClient != null ? WrappedObject.SourceClient.OH_FullName : ZString.Empty; }
		}

		public ZDateTime FirstRecognitionDate
		{
			get { return WrappedObject.FirstRecognitionDate; }
		}

		public ZString LocalCurrencyCode
		{
			get { return WrappedObject.LocalCurrencyCode; }
		}

		public ZDecimal LocalToPreferredExchangeRate
		{
			get { return WrappedObject.LocalToPreferredExchangeRate; }
		}

		public ZDecimal OutstandingEntityCommissionAmountInPreferredCurrency
		{
			get { return WrappedObject.OutstandingEntityCommissionAmountInPreferredCurrency; }
		}

		public ZDecimal PaidEntityCommissionAmountInPreferredCurrency
		{
			get { return WrappedObject.PaidEntityCommissionAmountInPreferredCurrency; }
		}

		public ZString PartyCode
		{
			get { return WrappedObject.Party != null ? WrappedObject.Party.OH_Code : ZString.Empty; }
		}

		public ZString PartyName
		{
			get { return WrappedObject.Party != null ? WrappedObject.Party.OH_FullName : ZString.Empty; }
		}

		public ZString PreferredCurrencyCode
		{
			get { return WrappedObject.PreferredCurrencyCode; }
		}

		public ZString PreferredPaymentCompanyCode
		{
			get { return WrappedObject.PreferredPaymentCompany != null ? WrappedObject.PreferredPaymentCompany.GC_Code : ZString.Empty; }
		}

		public ZDecimal ShareCommissionAmountInLocalCurrency
		{
			get { return WrappedObject.ShareCommissionAmountInLocalCurrency; }
		}

		public ZDecimal ShareCommissionAmountInPreferredCurrency
		{
			get { return WrappedObject.ShareCommissionAmountInPreferredCurrency; }
		}

		public ZString StaffCode
		{
			get { return WrappedObject.StaffCode; }
		}

		public ZString StaffName
		{
			get { return WrappedObject.StaffName; }
		}

		public ZDecimal TotalCommissionableAmountInLocalCurrency
		{
			get { return WrappedObject.TotalCommissionableAmountInLocalCurrency; }
		}

		public ZDecimal TotalCommissionableAmountInPreferredCurrency
		{
			get { return WrappedObject.TotalCommissionableAmountInPreferredCurrency; }
		}

		public ZDecimal TotalCommissionableAmountInTransactionCurrency
		{
			get { return WrappedObject.TotalCommissionableAmountInTransactionCurrency; }
		}

		public ZString TransactionCurrencyCode
		{
			get { return WrappedObject.TransactionCurrencyCode; }
		}
	}
}
