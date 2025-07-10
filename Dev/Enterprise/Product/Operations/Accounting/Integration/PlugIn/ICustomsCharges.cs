using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Accounting.Integration
{
	public interface ICustomsChargesAdditionalCharges
	{
		ICustomsCharges[] AdditionalCharges { get; }
	}

	/// <summary>
	/// Returns the description and total amount of all charges
	/// </summary>
	public interface ICustomsCharges
	{
		ZBool IsActive { get; }
		CustomsCharge[] GetCustomsCharges(ILogger logger);
	}

	/// <summary>
	/// AP is raised for each of this. CusEntryHeader or CusStatementLine
	/// AR is raised per IAccInvoiceDataProvider.CustomsJob against which a Accounting Job is created
	/// </summary>
	public interface IAccInvoiceDataProvider
	{
		/// <summary>
		/// For US statement, it returns false if there is no CusEntryHeader for the entry number 
		/// </summary>
		bool IsBillable { get; }

		string ReasonForUnbillability { get; }

		/// <summary>
		/// System sends an email if disbursement charges are posted.
		/// System deletes unposted disbursement charges.
		/// </summary>
		bool HasBeenWithdrawn { get; }

		/// <summary>
		/// In any other countries, withdrawn. For US, withdrawal is used for ex-warehouse. It should be 'deleted' instead.
		/// </summary>
		string EntryWithdrawnStatusTerm { get; }

		/// <summary>
		/// For EU AutomatichChargesCreation DueDateCalculation on base of Creditor
		/// </summary>
		bool IsAutoBillingDueDateFromPaymentTerms { get; }

		ICustomsCharges[] CustomsCharges { get; }
		ZString UniqueNumber { get; }
		ZString PreviousUniqueNumber { get; }
		ZDateTime InvoiceDate { get; }
		ZDateTime APDueDate { get; }

		AutoPostingNotification AutoPostingNotification { get; }

		ICustomsJobInfo CustomsJob { get; }

		BusinessObjectFactory Factory { get; }

		bool IsEligibleForIntegration { get; }

		bool APInvoiceNumberAlwaysIncludeChargeCode { get; }

		bool MatchCustomsChargesToClear(ZString apInvoiceNumber, ZString description);
	}
}
