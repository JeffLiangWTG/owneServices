using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Forwarding
	{
		public interface IForwardingShipmentProcessTask : IExceptionDurationProcessTask { }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IForwardingConsolProcessTask : IExceptionDurationProcessTask { }
		public interface IForwardingBulkCopyCriteria { }
		public interface IOrder { }
		public interface IOrderLine { }
		public interface IJobSupplierBookingCollection : IBusinessObjectCollection { }
		public interface IContainerLoadListHeaderCollection : IBusinessObjectCollection { }
	}
}
