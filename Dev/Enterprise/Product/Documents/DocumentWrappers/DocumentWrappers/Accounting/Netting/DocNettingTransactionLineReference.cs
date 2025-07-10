using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingTransactionLineReference : DocBaseWrapper
	{
		protected DocNettingTransactionLineReference(NettingTransactionLineReference nettingTransactionLineReference, BusinessObjectFactory factoryToWrap)
			: base(nettingTransactionLineReference, factoryToWrap)
		{
			Argument.NotNull(nettingTransactionLineReference, "NettingTransactionLineReference");
		}

		public static DocNettingTransactionLineReference New(NettingTransactionLineReference nettingTransactionLineReference, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingTransactionLineReference(nettingTransactionLineReference, factoryToWrap);
		}

		NettingTransactionLineReference TransactionLineReference => (NettingTransactionLineReference)WrappedObject;

		public ZString ShipmentNumbers
		{
			get { return TransactionLineReference.ShipmentNumbers; }
		}

		public ZString ConsolNumbers
		{
			get { return TransactionLineReference.ConsolNumbers; }
		}

		public ZString VesselVoyageNumbers
		{
			get { return TransactionLineReference.VesselVoyageNumbers; }
		}

		public ZString ConsolContainerNumbers
		{
			get { return TransactionLineReference.ConsolContainerNumbers; }
		}

		public ZString MasterWayBillNumbers
		{
			get { return TransactionLineReference.MasterWayBillNumbers; }
		}

		public ZString BookingConfirmationReferenceNumbers
		{
			get { return TransactionLineReference.BookingConfirmationReferenceNumbers; }
		}

		public ZString AgentReferenceNumbers
		{
			get { return TransactionLineReference.AgentReferenceNumbers; }
		}

		public ZString PackingReferenceNumbers
		{
			get { return TransactionLineReference.PackingReferenceNumbers; }
		}

		public ZString HouseBillNumbers
		{
			get { return TransactionLineReference.HouseBillNumbers; }
		}

		public ZString OrderReferenceNumbers
		{
			get { return TransactionLineReference.OrderReferenceNumbers; }
		}
	}
}
