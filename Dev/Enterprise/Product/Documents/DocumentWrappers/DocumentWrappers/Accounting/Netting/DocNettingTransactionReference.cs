using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingTransactionReference : DocBaseWrapper
	{
		protected DocNettingTransactionReference(NettingTransactionReference nettingTransactionReference, BusinessObjectFactory factoryToWrap)
			: base(nettingTransactionReference, factoryToWrap)
		{
			Argument.NotNull(nettingTransactionReference, "NettingTransactionReference");
		}

		public static DocNettingTransactionReference New(NettingTransactionReference nettingTransactionReference, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingTransactionReference(nettingTransactionReference, factoryToWrap);
		}

		NettingTransactionReference TransactionReference => (NettingTransactionReference)WrappedObject;

		public ZString InvoiceNumber
		{
			get { return TransactionReference.InvoiceNumber; }
		}

		public ZString JobInvoiceNumber
		{
			get { return TransactionReference.JobInvoiceNumber; }
		}

		public ZString ShipmentNumber
		{
			get { return TransactionReference.ShipmentNumber; }
		}

		public ZString ConsolNumber
		{
			get { return TransactionReference.ConsolNumber; }
		}

		public ZString VesselVoyageNumber
		{
			get { return TransactionReference.VesselVoyageNumber; }
		}

		public ZString ConsolContainerNumber
		{
			get { return TransactionReference.ConsolContainerNumber; }
		}

		public ZString MasterWayBillNumber
		{
			get { return TransactionReference.MasterWayBillNumber; }
		}

		public ZString BookingConfirmationReferenceNumber
		{
			get { return TransactionReference.BookingConfirmationReferenceNumber; }
		}

		public ZString AgentReferenceNumber
		{
			get { return TransactionReference.AgentReferenceNumber; }
		}
	}
}
