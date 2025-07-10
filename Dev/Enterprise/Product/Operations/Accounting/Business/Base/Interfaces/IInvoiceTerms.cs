using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IInvoiceTerms
	{
		OrgHeader Header { get; }

		JobInvoicingConsumerType JobType { get; }

		ZString Direction { get; }

		ZString TransportMode { get; }

		ZGuid AH_GB { get; }

		ZGuid AH_GE { get; }

		ZString AH_TransactionCategory { get; }

		ZByte AH_InvoiceTermDays { get; set; }

		ZString AH_InvoiceTerm { get; set; }

		BusinessObjectFactory Factory { get; }

		ZPropertyInfo AH_InvoiceDateInfo { get; }

		ZPropertyInfo AH_InvoiceTermInfo { get; }

		ZPropertyInfo AH_InvoiceTermDaysInfo { get; }

		ZDateTime AH_DueDate { get; set; }

		ZDateTime AH_InvoiceDate { get; }

		ZDateTime AH_DocumentReceivedDate { get; }

		ZPropertyInfo AH_DocumentReceivedDateInfo { get; }

		JobHeader Job { get; }
	}
}
