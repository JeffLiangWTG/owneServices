using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IDataProviderWithNotifications
	{
		IAccInvoiceDataProvider DataProvider { get; }
		ZString[] Errors { get; }
		ZString[] Warnings { get; }
		ZBool HasDiscrepancyForAmount { get; }
		ZBool HasDiscrepancyForTaxAmount { get; }
		ZDecimal CustomsAmount { get; }
		ZDecimal CustomsTaxAmount { get; }
		ZDecimal InvoiceAmount { get; }
		ZDecimal InvoiceTaxAmount { get; }
	}

	public interface ICustomsPaymentCreationResult
	{
		ZBool WasSuccessful { get; }
		ZString ErrorMessage { get; }
		IDataProviderWithNotifications[] DataProvidersWithNotifications { get; }
	}

	public interface ICustomsPaymentDataProvider
	{
		ZString HyperLinkText { get; }

		//It may be an account number. In US, it is payer's unit number which is uniquely assigned to each bank account by US customs for their clients.
		ZString BankAccountUniqueID { get; }
		ZGuid BankAccountPK { get; }

		bool SendEmail { get; }
		ZDateTime PaymentDate { get; }
		ZGuid EmailGroupPK { get; }
		OrgHeader Creditor { get; }
		GlbCompany Company { get; }
	}

	public interface IAPPaymentGroup
	{
		ZString APPaymentNumber { get; }
		IEnumerable<IAccInvoiceDataProvider> InvoiceDataProviders { get; }
	}

	public interface ICustomsPaymentCreator
	{
		ICustomsPaymentCreationResult CreatePayment(ICustomsPaymentDataProvider paymentDataProvider, IEnumerable<IAPPaymentGroup> apPaymentGroups);
	}
}
