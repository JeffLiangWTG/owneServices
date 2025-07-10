using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface ICountryEInvoicingObjectFactorySettings
	{
		// 🚩 Please avoid adding to this interface. 🚩
		// Instead, add to ICountryEInvoicingObjectFactory in Accounting.ElectronicMessaging.
		// OR, add to AccountingCountryFactory in Accounting.Business.
		// OR, add to Accounting.CountryCompliance.

		ZString CountryCode { get; }
		ZString AuthorizationRecordType { get; }
		IEInvoicingCredentialSettings Credentials { get; }
		string ApTransactionListRequestBatchId { get; }
	}
}
