namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbBookingConsolidationProcessTask { }
	public interface IDtbBookingConsolidationFilterBusinessObject { }
	public interface IDtbBookingProcessTask { }
	public interface IDtbBookingInstructionProcessTask { }
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IDtbBookingConfirmationProcessTask { }
	public interface IViewTransportBookingParents { }
	public interface IDtbBookingConsolidationPkgPackageJob { }
	public interface ITransportBookingRegistryProvider
	{
		IRegistryItem TransportBookingChargeableFactor { get; }
	}
}
